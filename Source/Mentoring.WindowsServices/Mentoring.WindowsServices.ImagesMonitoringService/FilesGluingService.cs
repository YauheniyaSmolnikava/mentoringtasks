using System.IO;
using System;
using System.Threading;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using MigraDoc.DocumentObjectModel.Shapes;
using System.Linq;
using System.Collections.Generic;
using System.Timers;
using System.Diagnostics;
using Topshelf;
using Microsoft.ServiceBus.Messaging;
using Mentoring.WindowsServices.Utils;
using ZXing;
using Mentoring.WindowsServices.Utils.Interfaces;
using Mentoring.WindowsServices.CentralizedResultsCollection.IoC;

namespace Mentoring.WindowsServices.ImagesMonitoringService
{
    public class FilesGluingService : ServiceControl
    {
        #region Fields

        FileSystemWatcher watcher;
        Thread workingThread;
        Thread settingsThread;

        string inDir;
        string outSuccessDir;
        string outFailedDir;
        string barcodeVal;

        string queueName = "FileQueue";
        string topicName = "ServiceStatusTopic";
        string subscriptionName = "ServiceSettingsSubscription";
        string subscriptionTopicName = "ServiceSettingsTopic";

        ManualResetEvent workStop;
        AutoResetEvent newFile;
        QueueClient queueClient;
        TopicClient topicClient;
        SubscriptionClient subscriptionClient;

        Document document;
        Section section;
        System.Timers.Timer timer;
        System.Timers.Timer statusTimer;

        //acceptable formats
        string[] extensions = new string[] { ".png", ".jpg", ".jpeg" };

        //images that was written to document but not saved to pdf
        List<string> processedImages;
        List<int> processedImagesNumeration;
        bool firstImage;
        bool stop;
        BarcodeReader barcodeReader;
        ServiceStatus serviceStatus;

        IAzureHelper AzureHelper;
        IFileHelper FileHelper;

        #endregion

        #region Public Methods & Constructors

        public FilesGluingService()
        {
            var curDir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

            inDir = Path.Combine(curDir, "in");
            outSuccessDir = Path.Combine(curDir, "successOut");
            outFailedDir = Path.Combine(curDir, "failedOut");

            if (!Directory.Exists(inDir))
                Directory.CreateDirectory(inDir);

            if (!Directory.Exists(outSuccessDir))
                Directory.CreateDirectory(outSuccessDir);

            if (!Directory.Exists(outFailedDir))
                Directory.CreateDirectory(outFailedDir);

            DependencyResolver.Initialize();

            AzureHelper = DependencyResolver.For<IAzureHelper>();
            FileHelper = DependencyResolver.For<IFileHelper>();

            stop = false;
            workingThread = new Thread(ImagesMonitoring);
            settingsThread = new Thread(ServiceSettingsMonitoring);
            workStop = new ManualResetEvent(false);
            newFile = new AutoResetEvent(false);

            watcher = new FileSystemWatcher(inDir);
            watcher.Created += FileAppeared;

            barcodeVal = "Document Breaker";
            serviceStatus = new ServiceStatus { Status = ServiceStatusEnum.WaitingForNewFiles, Timeout = 15000, Barcode = barcodeVal };

            CreateNewDocument();
            timer = new System.Timers.Timer(15000);
            timer.Elapsed += OnTimedEvent;

            statusTimer = new System.Timers.Timer(10000);
            statusTimer.Elapsed += SendUpdatedStatusEvent;

            queueClient = QueueClient.Create(queueName);
            topicClient = TopicClient.Create(topicName);
            subscriptionClient = SubscriptionClient.Create(subscriptionTopicName, subscriptionName);

            barcodeReader = new BarcodeReader { AutoRotate = true };
        }

        public bool Start(HostControl hostControl)
        {
            workingThread.Start();
            settingsThread.Start();
            watcher.EnableRaisingEvents = true;
            serviceStatus.Status = ServiceStatusEnum.WaitingForNewFiles;
            statusTimer.Start();
            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            watcher.EnableRaisingEvents = false;
            stop = true;
            workStop.Set();
            workingThread.Join();
            settingsThread.Join();
            return true;
        }

        #endregion

        #region Private Methods

        private void ImagesMonitoring()
        {
            do
            {
                timer.Stop();

                foreach (var file in Directory.EnumerateFiles(inDir))
                {
                    serviceStatus.Status = ServiceStatusEnum.FileProcessing;

                    if (workStop.WaitOne(TimeSpan.Zero))
                        return;

                    //if file available write image to document
                    if (FileHelper.TryOpen(file, 3))
                    {
                        WriteImageToDocument(file);
                    }
                }

                //timer for detecting if new document should be created
                timer.Start();

                serviceStatus.Status = ServiceStatusEnum.WaitingForNewFiles;
            }
            while (WaitHandle.WaitAny(new WaitHandle[] { workStop, newFile }) != 0);

            serviceStatus.Status = ServiceStatusEnum.Stopping;

            //saving all processed images before stopping service
            SaveImagesToPdfDocumentAndSendToQueue();
        }

        private async void ServiceSettingsMonitoring()
        {
            // listen to messages from topic, changes in settings and applying this changes
            do
            {
                var brokeredMsg = await subscriptionClient.ReceiveAsync();
                if (brokeredMsg != null)
                {
                    var settings = brokeredMsg.GetBody<Settings>();

                    if (settings.UpdateStatus)
                    {
                        SendUpdatedStatus();
                    }

                    timer.Interval = settings.Timeout;
                    barcodeVal = settings.Barcode;

                    brokeredMsg.Complete();
                }

            } while (!stop);
        }

        private void CreateNewDocument()
        {
            //saving processed images before new document creating
            SaveImagesToPdfDocumentAndSendToQueue();
            document = new Document();
            section = document.AddSection();
            processedImages = new List<string>();
            processedImagesNumeration = new List<int>();
            firstImage = true;
        }

        private void WriteImageToDocument(string file)
        {
            //checing acceptable extensions
            if (extensions.Any(ex => ex == Path.GetExtension(file)))
            {
                //check if this file was already written to document
                if (!processedImages.Any(img => img == file))
                {
                    var imgNumber = FileHelper.GetImageNumeration(file);

                    var checkBarcode = FileHelper.CheckBarCode(file, barcodeReader, barcodeVal);

                    //if numeration was broken or barcode was found - create new document
                    if ((processedImagesNumeration.Count() != 0 && processedImagesNumeration.Max() != (imgNumber - 1)) || checkBarcode)
                    {
                        CreateNewDocument();
                    }

                    if (!checkBarcode)
                    {
                        //add page break before adding new image to document
                        if (!firstImage)
                        {
                            section.AddPageBreak();
                        }

                        var img = section.AddImage(file);

                        img.RelativeHorizontal = RelativeHorizontal.Page;
                        img.RelativeVertical = RelativeVertical.Page;

                        img.Top = 0;
                        img.Left = 0;

                        img.Height = document.DefaultPageSetup.PageHeight;
                        img.Width = document.DefaultPageSetup.PageWidth;

                        processedImages.Add(file);
                        processedImagesNumeration.Add(imgNumber);

                        firstImage = false;
                    }
                    else
                    {
                        //delete image with barcode
                        File.Delete(Path.Combine(inDir, Path.GetFileName(file)));
                    }
                }
            }
            else
            {
                //moving file with wrong extension to specified folder
                if (FileHelper.TryOpen(file, 3))
                {
                    MoveOrDeleteFile(file);
                }
            }
        }

        private void SaveImagesToPdfDocumentAndSendToQueue()
        {
            serviceStatus.Status = ServiceStatusEnum.DocumentSending;

            if (processedImages != null && processedImages.Count() > 0)
            {
                try
                {
                    var render = new PdfDocumentRenderer();
                    render.Document = document;

                    render.RenderDocument();
                    var doc = render.PdfDocument;

                    //obtaining bytes of prepared document and sending azure message to queue
                    MemoryStream stream = new MemoryStream();
                    doc.Save(stream);
                    byte[] docBytes = stream.ToArray();

                    var key = Guid.NewGuid();
                    var messages = AzureHelper.ConstructAzureMessage(docBytes);
                    IEnumerable<BrokeredMessage> brokeredMessages = messages
                        .Select(msg => new BrokeredMessage(msg) { PartitionKey = key.ToString() });

                    foreach (var msg in brokeredMessages)
                    {
                        queueClient.Send(msg);
                    }
                }
                catch
                {
                    foreach (var file in processedImages)
                    {
                        //moving to failed folder
                        MoveOrDeleteFile(file);
                    }
                }

                serviceStatus.Status = ServiceStatusEnum.DeletingFiles;

                foreach (var img in processedImages)
                {
                    //deleting processed files
                    if (FileHelper.TryOpen(img, 3))
                    {
                        File.Delete(Path.Combine(inDir, Path.GetFileName(img)));
                    }
                }
            }
        }

        private void MoveOrDeleteFile(string file)
        {
            serviceStatus.Status = ServiceStatusEnum.DeletingFiles;

            if (FileHelper.TryOpen(file, 3))
            {
                if (!File.Exists(Path.Combine(outFailedDir, Path.GetFileName(file))))
                {
                    File.Move(file, Path.Combine(outFailedDir, Path.GetFileName(file)));
                }
                else
                {
                    File.Delete(Path.Combine(inDir, Path.GetFileName(file)));
                }
            }
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            //creating new document on timer event
            CreateNewDocument();
        }

        private void SendUpdatedStatusEvent(Object source, ElapsedEventArgs e)
        {
            SendUpdatedStatus();
        }

        private void SendUpdatedStatus()
        {
            //sending current status to topic
            serviceStatus.Timeout = (int)timer.Interval;
            serviceStatus.Barcode = barcodeVal;
            var statusMsg = new BrokeredMessage(serviceStatus);
            topicClient.Send(statusMsg);
        }

        private void FileAppeared(object sender, FileSystemEventArgs e)
        {
            newFile.Set();
        }

        #endregion
    }
}

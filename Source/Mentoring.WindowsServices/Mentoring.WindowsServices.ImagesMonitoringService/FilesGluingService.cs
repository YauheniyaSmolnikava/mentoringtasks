using System.IO;
using System;
using System.Threading;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using MigraDoc.DocumentObjectModel.Shapes;
using System.Linq;
using System.Collections.Generic;
using System.Timers;
using ZXing;
using System.Drawing;
using System.Diagnostics;
using Topshelf;

namespace Mentoring.WindowsServices.ImagesMonitoringService
{
    public class FilesGluingService : ServiceControl
    {
        #region Fields

        FileSystemWatcher watcher;
        Thread workingThread;

        string inDir;
        string outSuccessDir;
        string outFailedDir;

        ManualResetEvent workStop;
        AutoResetEvent newFile;

        Document document;
        Section section;
        System.Timers.Timer timer;

        //acceptable formats
        string[] extensions = new string[] { ".png", ".jpg", ".jpeg" };

        //images that was written to document but not saved to pdf
        List<string> processedImages;
        List<int> processedImagesNumeration;
        bool firstImage;
        BarcodeReader barcodeReader;

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

            workingThread = new Thread(ImagesMonitoring);
            workStop = new ManualResetEvent(false);
            newFile = new AutoResetEvent(false);

            watcher = new FileSystemWatcher(inDir);
            watcher.Created += FileAppeared;

            CreateNewDocument();
            timer = new System.Timers.Timer(15000);
            timer.Elapsed += OnTimedEvent;

            barcodeReader = new BarcodeReader { AutoRotate = true };
        }

        public bool Start(HostControl hostControl)
        {
            workingThread.Start();
            watcher.EnableRaisingEvents = true;
            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            watcher.EnableRaisingEvents = false;
            workStop.Set();
            workingThread.Join();
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
            }
            while (WaitHandle.WaitAny(new WaitHandle[] { workStop, newFile }) != 0);

            //saving all processed images before stopping service
            SaveImagesToPdfDocument();
        }

        private void CreateNewDocument()
        {
            //saving processed images before new document creating
            SaveImagesToPdfDocument();
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

                    var checkBarcode = FileHelper.CheckBarCode(file, barcodeReader);

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

        private void SaveImagesToPdfDocument()
        {
            if (processedImages != null && processedImages.Count() > 0)
            {
                try
                {
                    var render = new PdfDocumentRenderer();
                    render.Document = document;

                    render.RenderDocument();
                    render.Save(Path.Combine(outSuccessDir, Guid.NewGuid() + ".pdf"));
                }
                catch
                {
                    foreach (var file in processedImages)
                    {
                        //moving to failed folder
                        MoveOrDeleteFile(file);
                    }
                }

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

        private void FileAppeared(object sender, FileSystemEventArgs e)
        {
            newFile.Set();
        }

        #endregion
    }
}

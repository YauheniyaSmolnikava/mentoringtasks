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

namespace Mentoring.WindowsServices.ImagesMonitoringService
{
    public class FilesGluingService
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

        string[] extensions = new string[] { ".png", ".jpg", ".jpeg" };
        List<string> processedImages;
        List<int> processedImagesNumeration;
        BarcodeReader barcodeReader;

        #endregion

        #region Public Methods & Constructors

        public FilesGluingService(string inDir, string outSuccessDir, string outFailedDir)
        {
            this.inDir = inDir;
            this.outSuccessDir = outSuccessDir;
            this.outFailedDir = outFailedDir;

            if (!Directory.Exists(inDir))
                Directory.CreateDirectory(inDir);

            if (!Directory.Exists(outSuccessDir))
                Directory.CreateDirectory(outSuccessDir);

            if (!Directory.Exists(outFailedDir))
                Directory.CreateDirectory(outFailedDir);

            workingThread = new Thread(WorkProc);
            workStop = new ManualResetEvent(false);
            newFile = new AutoResetEvent(false);

            watcher = new FileSystemWatcher(inDir);
            watcher.Created += FileAppeared;

            CreateNewDocument();
            timer = new System.Timers.Timer(15000);
            timer.Elapsed += OnTimedEvent;

            barcodeReader = new BarcodeReader { AutoRotate = true };
        }

        public void Start()
        {
            workingThread.Start();
            watcher.EnableRaisingEvents = true;
        }

        public void Stop()
        {
            watcher.EnableRaisingEvents = false;
            workStop.Set();
            workingThread.Join();
        }

        #endregion

        #region Private Methods
        private void WorkProc()
        {
            do
            {
                timer.Stop();

                foreach (var file in Directory.EnumerateFiles(inDir))
                {
                    if (workStop.WaitOne(TimeSpan.Zero))
                        return;

                    if (TryOpen(file, 3))
                    {
                        WriteImageToDocument(file);
                    }
                }

                timer.Start();
            }
            while (WaitHandle.WaitAny(new WaitHandle[] { workStop, newFile }) != 0);
            SaveImagesToPdfDocument();
        }

        private void WriteImageToDocument(string file)
        {
            if (extensions.Any(ex => ex == Path.GetExtension(file)))
            {
                if (!processedImages.Any(img => img == file))
                {
                    var imgNumber = GetImageNumeration(file);

                    var checkBarcode = CheckBarCode(file);
                    if ((processedImagesNumeration.Count() != 0 && processedImagesNumeration.Max() != (imgNumber - 1)) || checkBarcode)
                    {
                        CreateNewDocument();
                    }

                    if (!checkBarcode)
                    {
                        var img = section.AddImage(file);

                        img.RelativeHorizontal = RelativeHorizontal.Page;
                        img.RelativeVertical = RelativeVertical.Page;

                        img.Top = 0;
                        img.Left = 0;

                        img.Height = document.DefaultPageSetup.PageHeight;
                        img.Width = document.DefaultPageSetup.PageWidth;

                        processedImages.Add(file);
                        processedImagesNumeration.Add(imgNumber);

                        section.AddPageBreak();
                    }
                    else
                    {
                        File.Delete(Path.Combine(inDir, Path.GetFileName(file)));
                    }
                }
            }
            else
            {
                File.Move(file, Path.Combine(outFailedDir, Path.GetFileName(file)));
            }
        }

        private int GetImageNumeration(string file)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            var fileParts = fileName.Split(new[] { '_' });

            if (fileParts.Count() > 1)
            {
                int imgNumber;
                return int.TryParse(fileParts.Last(), out imgNumber) ? imgNumber : -1;
            }

            return -1;
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            SaveImagesToPdfDocument();
            CreateNewDocument();
        }

        private void CreateNewDocument()
        {
            SaveImagesToPdfDocument();
            document = new Document();
            section = document.AddSection();
            processedImages = new List<string>();
            processedImagesNumeration = new List<int>();
        }

        private void SaveImagesToPdfDocument()
        {
            if (processedImages != null && processedImages.Count() > 0)
            {
                var render = new PdfDocumentRenderer();
                render.Document = document;

                render.RenderDocument();
                render.Save(Path.Combine(outSuccessDir, Guid.NewGuid() + ".pdf"));

                foreach (var img in processedImages)
                {
                    File.Delete(Path.Combine(inDir, Path.GetFileName(img)));
                }
            }
        }

        private void FileAppeared(object sender, FileSystemEventArgs e)
        {
            newFile.Set();
        }

        private bool TryOpen(string fullPath, int v)
        {
            for (int i = 0; i < v; i++)
            {
                try
                {
                    var file = File.Open(fullPath, FileMode.Open, FileAccess.Read, FileShare.None);
                    file.Close();

                    return true;
                }
                catch (IOException)
                {
                    Thread.Sleep(3000);
                }
            }

            return false;
        }

        private bool CheckBarCode(string file)
        {
            Result result;

            using (var bmp = (Bitmap)Bitmap.FromFile(file))
            {
                result = barcodeReader.Decode(bmp);
            }

            return result != null && result.BarcodeFormat == BarcodeFormat.CODE_128 && result.Text == "Document Breaker";
        }

        #endregion
    }
}

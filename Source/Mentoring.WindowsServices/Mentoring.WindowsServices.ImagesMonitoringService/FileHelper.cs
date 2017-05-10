using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using ZXing;

namespace Mentoring.WindowsServices.ImagesMonitoringService
{
    public static class FileHelper
    {
        public static int GetImageNumeration(string file)
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

        public static bool TryOpen(string fullPath, int v)
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

        public static bool CheckBarCode(string file, BarcodeReader barcodeReader)
        {
            try
            {
                Result result;

                using (var bmp = (Bitmap)Bitmap.FromFile(file))
                {
                    result = barcodeReader.Decode(bmp);
                }

                return result != null && result.BarcodeFormat == BarcodeFormat.CODE_128 && result.Text == "Document Breaker";
            }
            catch
            {
                return false;
            }
        }
    }
}

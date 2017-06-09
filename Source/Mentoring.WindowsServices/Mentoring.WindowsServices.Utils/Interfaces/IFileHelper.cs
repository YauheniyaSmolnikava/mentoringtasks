
using ZXing;

namespace Mentoring.WindowsServices.Utils.Interfaces
{
    public interface IFileHelper
    {
        int GetImageNumeration(string file);

        bool TryOpen(string fullPath, int v);

        bool CheckBarCode(string file, BarcodeReader barcodeReader, string barcodeVal);
    }
}

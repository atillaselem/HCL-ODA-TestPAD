using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Media;

namespace HCL_ODA_TestPAD.HCL;

public interface ICadImageViewBitmapService
{
    WriteableBitmap GetWritableBitmap(bool isVisible);
    WriteableBitmap GetWritableBitmap(Size scaledSize, uint dpiX, uint dpiY);
    Int32Rect UpdateImageArea();
}

public class CadImageViewBitmapService : ICadImageViewBitmapService
{
    private WriteableBitmap _writeableBitmap;
    private Size _scaledSize;

    public WriteableBitmap GetWritableBitmap(bool isVisible)
    {
        return isVisible ? _writeableBitmap : null;
    }

    public WriteableBitmap GetWritableBitmap(Size scaledSize, uint dpiX, uint dpiY)
    {
        if (scaledSize.Width <= 0 || scaledSize.Height <= 0)
        {
            return null;
        }

        _writeableBitmap = new WriteableBitmap((int)scaledSize.Width, (int)scaledSize.Height, dpiX, dpiY,
            PixelFormats.Bgr24, null);
        _scaledSize = scaledSize;
        return _writeableBitmap;
    }

    public Int32Rect UpdateImageArea()
    {
        if (_writeableBitmap != null)
        {
            var dirtyRect = new Int32Rect(0, 0, (int)_scaledSize.Width, (int)_scaledSize.Height);
            _writeableBitmap.Lock();
            _writeableBitmap.AddDirtyRect(dirtyRect);
            _writeableBitmap.Unlock();
            return dirtyRect;
        }

        return default;
    }

}

using System;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using Teigha.Core;
using Teigha.Visualize;
using Size = System.Windows.Size;

namespace HCL_ODA_TestPAD.HCL;

public interface ILogger
{

}
internal class HPLLogger : ILogger
{
}
public interface ICadModel : IDisposable
{
    event EventHandler ViewUpdateRequested;
    void UpdateImageBuffer(WriteableBitmap writableBitmap);
    void ViewSizeChanged(Size size);
    void UpdateWritableBitmap();
    void Update(bool invalidate);
}
public class CadModel : ICadModel
{
    /// <summary>
    /// OpenGLES2 Writeable
    /// </summary>
    public void UpdateWritableBitmap()
    {
        if (WritableBackBuffer != IntPtr.Zero)
        {
            using var pDev = TvGsDeviceId.openObject(OpenMode.kForWrite);
            using var pRastImg = pDev?.getRasterImage();
            if (pRastImg != null)
            {
                byte[] bufferedRasterImage = _rasterImageBufffer.GetBuffer(pRastImg);
                pRastImg.scanLines(ref bufferedRasterImage, 0, _rasterImageBufffer.NumberOfLines);
                Marshal.Copy(bufferedRasterImage, 0, WritableBackBuffer, bufferedRasterImage.Length);
            }
        }
    }

    private IntPtr WritableBackBuffer;
    public OdTvGsDeviceId TvGsDeviceId { get; set; }

    public event EventHandler ViewUpdateRequested;
    public RasterImageBuffer _rasterImageBufffer = new();

    public void UpdateImageBuffer(WriteableBitmap writableBitmap)
    {
        if (writableBitmap == null)
        {
            WritableBackBuffer = IntPtr.Zero;
            _rasterImageBufffer.ResetBuffer();
            return;
        }
        WritableBackBuffer = writableBitmap.BackBuffer;
    }

    public void ViewSizeChanged(Size size)
    {
        if (TvGsDeviceId != null && !TvGsDeviceId.isNull())
        {
            using var odTvGsDevice = TvGsDeviceId.openObject(OpenMode.kForWrite);
            using var rect = new OdTvDCRect(0, (int)size.Width, (int)size.Height, 0);
            odTvGsDevice.onSize(rect);
            odTvGsDevice.invalidate();
            odTvGsDevice.update();
        }

    }

    public void Update(bool invalidate)
    {
        if (TvGsDeviceId != null && !TvGsDeviceId.isNull())
        {
            using var odTvGsDevice = TvGsDeviceId.openObject(OpenMode.kForWrite);

            if (odTvGsDevice.isValid() && invalidate == false)
            {
                return;
            }

            if (invalidate)
            {
                odTvGsDevice.invalidate();
            }
            odTvGsDevice.update();
            ViewUpdateRequested?.Invoke(this, null);
        }
    }

    public void Dispose()
    {
        TvGsDeviceId = null;
        ViewUpdateRequested = null;
        WritableBackBuffer = IntPtr.Zero;
    }
}

public class RasterImageBuffer
{
    private uint _lineSize;
    private bool _isBufferCreated;
    private byte[] _imageBuffer;
    private uint _numberOfLines;
    public  uint NumberOfLines => _numberOfLines;

    public byte[] GetBuffer(OdGiRasterImage rasterImage)
    {
        if (rasterImage == null)
        {
            return _imageBuffer;
        }

        var pixelHeight = rasterImage.pixelHeight();
        var lineSize = rasterImage.scanLineSize();
        if (!_isBufferCreated || _numberOfLines != pixelHeight || _lineSize != lineSize)
        {
            _numberOfLines = pixelHeight;
            _lineSize = lineSize;
            _imageBuffer = new byte[(int)(pixelHeight * lineSize)];
            _isBufferCreated = true;
        }
        return _imageBuffer;
    }
    public void ResetBuffer()
    {
        _isBufferCreated = false;
    }
}
public static class RasterImageExtensions
{
    private static uint _lineSize;
    private static bool _isBufferCreated;
    private static byte[] _imageBuffer;
    private static uint _numberOfLines;
    public static uint NumberOfLines => _numberOfLines;

    public static byte[] GetBuffer(this OdGiRasterImage rasterImage)
    {
        if (rasterImage == null)
        {
            return _imageBuffer;
        }

        var pixelHeight = rasterImage.pixelHeight();
        var lineSize = rasterImage.scanLineSize();
        if (!_isBufferCreated || _numberOfLines != pixelHeight || _lineSize != lineSize)
        {
            _numberOfLines = pixelHeight;
            _lineSize = lineSize;
            _imageBuffer = new byte[(int)(pixelHeight * lineSize)];
            _isBufferCreated = true;
        }
        return _imageBuffer;
    }
    public static void ResetBuffer()
    {
        _isBufferCreated = false;
    }
}

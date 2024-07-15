using ODA.Kernel.TD_RootIntegrated;

namespace HCL_ODA_TestPAD.HCL;

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
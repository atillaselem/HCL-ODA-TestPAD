using HCL_ODA_TestPAD.Utility;
using Newtonsoft.Json.Linq;
using System;
using System.Runtime;
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
            using var pDev = TvGsDeviceId.openObject(OpenMode.kForRead);
            using var pRastImg = pDev?.getRasterImage();
            if (pRastImg != null)
            {
                byte[] bufferedRasterImage = pRastImg.scanLines();
                Marshal.Copy(bufferedRasterImage, 0, WritableBackBuffer, bufferedRasterImage.Length);
            }
        }
    }

    private IntPtr WritableBackBuffer;
    public OdTvGsDeviceId TvGsDeviceId { get; set; }

    public event EventHandler ViewUpdateRequested;
    private Func<CadRegenerator> _cadRegenFactory;

    public CadModel(Func<CadRegenerator> cadRegenFactory)
    {
        _cadRegenFactory = cadRegenFactory;
    }

    public void UpdateImageBuffer(WriteableBitmap writableBitmap)
    {
        if (writableBitmap == null)
        {
            WritableBackBuffer = IntPtr.Zero;
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
            odTvGsDevice.regen(OdTvGsDevice.RegenMode.kRegenVisible);
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

            odTvGsDevice.TryAutoRegeneration(_cadRegenFactory).update();
            ViewUpdateRequested?.Invoke(this, null);
        }
    }

    public void Dispose()
    {
        ViewUpdateRequested = null;
        WritableBackBuffer = IntPtr.Zero;
        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
        GC.Collect(GC.MaxGeneration);
    }
}



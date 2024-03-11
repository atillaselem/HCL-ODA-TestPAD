using System;
using System.Collections.Generic;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using ODA.Visualize.TV_Visualize;
using Size = System.Windows.Size;

namespace HCL_ODA_TestPAD.HCL;

public interface ILogger
{

}
internal class HPLLogger : ILogger
{
}

public interface ICadBehavior : IDisposable
{
    void Init(dynamic point);
    void Run(dynamic point);
    void Finish(dynamic point);
}
public enum CadViewOperation
{
    Pan,
    ZoomToScale,    
    ZoomToArea,
    Orbit
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
            using var pDev = TvGsDeviceId.openObject(OdTv_OpenMode.kForRead);
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
    private readonly Dictionary<CadViewOperation, ICadBehavior> _behaviourDict = new();
    public CadModel(Func<CadRegenerator> cadRegenFactory)
    {
        _cadRegenFactory = cadRegenFactory;
    }

    public void UpdateImageBuffer(WriteableBitmap writableBitmap)
    {
        if (TvGsDeviceId != null && !TvGsDeviceId.isNull() && writableBitmap != null)
        {
            using var odTvGsDevice = TvGsDeviceId.openObject(OdTv_OpenMode.kForWrite);
            odTvGsDevice.setDirectRenderBuffer(writableBitmap.BackBuffer);
            Update(false);
        }
    }

    private bool _isViewResized;
    public void ViewSizeChanged(Size size)
    {
        if (TvGsDeviceId != null && !TvGsDeviceId.isNull())
        {
            {
                using var odTvGsDevice = TvGsDeviceId.openObject(OdTv_OpenMode.kForWrite);
                using var rect = new OdTvDCRect(0, (int)size.Width, (int)size.Height, 0);
                odTvGsDevice.onSize(rect);
                odTvGsDevice.invalidate();
                odTvGsDevice.regen(OdTvGsDevice_RegenMode.kRegenVisible);
                _isViewResized = true;
            }
            Update(false);
        }

    }

    public void Update(bool invalidate)
    {
        if (_isViewResized && TvGsDeviceId != null && !TvGsDeviceId.isNull())
        {
            using var odTvGsDevice = TvGsDeviceId.openObject(OdTv_OpenMode.kForWrite);

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



using System;
using System.Windows;
using HCL_ODA_TestPAD.ViewModels.Base;

namespace HCL_ODA_TestPAD.HCL;


public class CadImageViewModel
{
    private readonly ICadModel _cadModel;
    private readonly ICadImageViewControl _control;
    private readonly ILogger _logger;
    private readonly ICadImageViewBitmapService _cadImageViewBitmapService;

    public CadImageViewModel(ICadImageViewControl control,
                             ILogger logger,
                             ICadModel cadModel,
                             ICadImageViewBitmapService cadImageViewBitmapService)
    {
        _control = control;
        _logger = logger;
        _cadModel = cadModel;
        _cadImageViewBitmapService = cadImageViewBitmapService;
    }

    public void OnVisibilityChanged(bool isVisible)
    {
        //_logger.LogInformation("CadImageViewControl visibility changed to: {IsVisible}", isVisible);
        _cadModel.UpdateImageBuffer(_cadImageViewBitmapService.GetWritableBitmap(isVisible));
        if (isVisible)
        {
            _cadModel.ViewUpdateRequested += OnCadImageViewUpdateRequested;
        }
        else
        {
            _cadModel.ViewUpdateRequested -= OnCadImageViewUpdateRequested;
        }
    }

    private void OnCadImageViewUpdateRequested(object sender, EventArgs e)
    {
        _control.InvalidateControl();
    }

    public void Update()
    {
        //_cadModel.Render();
        _cadModel.UpdateWritableBitmap();
        _cadImageViewBitmapService.UpdateImageArea();
    }

    public void OnRenderSizeChanged(Size newSize)
    {
        var (scaledSize, dpiX, dpiY) = GetScaledSize(newSize);
        _cadModel.ViewSizeChanged(scaledSize);
        CreateWritableBackBuffer(scaledSize, dpiX, dpiY);
    }

    public static (Size, uint, uint) GetScaledSize(Size newSize)
    {
        new CadScreenInfoProvider().GetEffectiveDpi(out var dpiX, out var dpiY);
        const double defaultImageDpi = 96;
        double scaleDpiX = dpiX / defaultImageDpi;
        double scaleDpiY = dpiY / defaultImageDpi;
        var scaledSize = new Size(newSize.Width * scaleDpiX, newSize.Height * scaleDpiY);
        return (scaledSize, dpiX, dpiY);
    }
    public void CreateWritableBackBuffer(Size scaledSize, uint dpiX, uint dpiY)
    {
        var writableBitmap = _cadImageViewBitmapService.GetWritableBitmap(scaledSize, dpiX, dpiY);
        _control.SetImageSource(writableBitmap);
        _cadModel.UpdateImageBuffer(writableBitmap);
    }

}

using System;
using System.Windows.Media.Imaging;
using ODA.Visualize.TV_Visualize;

namespace HCL_ODA_TestPAD.ViewModels.Base;

public interface ICadImageViewControl
{
    void InvalidateControl();
    void SetImageSource(WriteableBitmap writableBitmap);
    void SetFileLoaded(bool isFileLoaded, string filePath, Action<string> emitEvent);
    void SetRenderMode(OdTvGsView_RenderMode renderMode) { }
    void SetRenderModeButton(OdTvGsView_RenderMode model) { }
    void UpdateView() {}
}

using Prism.Events;
using System;
using System.Windows.Media.Imaging;
using Teigha.Visualize;

namespace HCL_ODA_TestPAD.ViewModels.Base;

public interface ICadImageViewControl
{
    void InvalidateControl();
    void SetImageSource(WriteableBitmap writableBitmap);
    void SetFileLoaded(bool isFileLoaded, string filePath, Action<string> emitEvent);
    void SetRenderMode(OdTvGsView.RenderMode renderMode) { }
    void SetRenderModeButton(OdTvGsView.RenderMode model) { }
}

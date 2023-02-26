using System.Windows.Media.Imaging;
using Teigha.Visualize;

namespace HCL_ODA_TestPAD.ViewModels.Base;

public interface ICadImageViewControl
{
    void InvalidateControl();
    void SetImageSource(WriteableBitmap writableBitmap);
    void SetFileLoaded(bool isFileLoaded, string filePath);
    void SetRenderModeButton(OdTvGsView.RenderMode mode);
}

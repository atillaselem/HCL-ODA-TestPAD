using System.Threading.Tasks;
using Teigha.Visualize;

namespace HCL_ODA_TestPAD.ViewModels.Base
{
    public interface ICadImageTabViewModel
    {
        Task LoadCadModelViewAsync();
        string TabItemTitle { get; set; }
        string CadImageFilePath { get; set; }
        void OnPanClicked();
        void OnOrbitClicked();
        void OnZoomClicked(ZoomType type);
        void OnSet3DViewClicked(OdTvExtendedView.e3DViewType type);
        void OnRenderModeClicked(OdTvGsView.RenderMode renderMode);
        void OnSetProjectionClicked(OdTvGsView.Projection projection);
        void OnRegenModeClicked(OdTvGsDevice.RegenMode regenMode);
        void OnRegenViewClicked();
        void CloseTabView();
    }
    public enum DeviceType
    {
        BitmapDevice,
        OnScreenDevice
    }
}

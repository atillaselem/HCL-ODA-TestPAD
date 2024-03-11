using System.Threading.Tasks;
using ODA.Visualize.TV_Visualize;
using ODA.Visualize.TV_VisualizeTools;

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
        void OnSet3DViewClicked(OdTvExtendedView_e3DViewType type);
        void OnRenderModeClicked(OdTvGsView_RenderMode renderMode);
        void OnSetProjectionClicked(OdTvGsView_Projection projection);
        void OnRegenModeClicked(OdTvGsDevice_RegenMode regenMode);
        void OnRegenViewClicked();
        void CloseTabView();
    }
    public enum DeviceType
    {
        BitmapDevice,
        OnScreenDevice
    }
}

using ODA.Visualize.TV_Visualize;
using ODA.Visualize.TV_VisualizeTools;


namespace HCL_ODA_TestPAD.ViewModels.Base
{
    public enum HclToolType
    {
        PLTStation,
        Prism,
        Points
    }
    public enum ZoomType
    {
        ZoomIn,
        ZoomOut,
        ZoomExtents,
        ZoomToArea
    }
    public enum ButtonName
    {
        PanBtn,
        OrbitBtn,
        IsometricBtn,
        PerspectiveBtn,
        Wireframe2DBtn,
        Wireframe3DBtn,
        HiddenLineBtn,
        ShadedBtn,
        ShadedWithEdgesBtn
    }
    public interface ICadOdaMenuView
    {
        void Pan();
        void Orbit();
        void SetZoom(ZoomType type);
        void Set3DView(OdTvExtendedView_e3DViewType type);
        void SetRenderMode(OdTvGsView_RenderMode renderMode);
        void SetProjectionType(OdTvGsView_Projection projection);
        void Regen(OdTvGsDevice_RegenMode regenMode);
        void RegenView();
    }

}

using Teigha.Visualize;


namespace HCL_ODA_TestPAD.ViewModels.Base
{
    public enum ZoomType
    {
        ZoomIn,
        ZoomOut,
        ZoomExtents
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
        void Set3DView(OdTvExtendedView.e3DViewType type);
        void SetRenderMode(OdTvGsView.RenderMode renderMode);
        void SetProjectionType(OdTvGsView.Projection projection);
        void Regen(OdTvGsDevice.RegenMode regenMode);
        void RegenView();
    }

}

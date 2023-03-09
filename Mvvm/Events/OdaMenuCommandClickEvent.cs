using Prism.Events;
using Teigha.Visualize;
using HCL_ODA_TestPAD.ViewModels.Base;

namespace HCL_ODA_TestPAD.Mvvm.Events
{
    public class OdaMenuCommandClickEvent : PubSubEvent<OdaMenuCommandClickEventArg>{}

    public class OdaMenuCommandClickEventArg
    {
        public OdaEventType OdaEventType { get; set; }
        public ZoomType eZoomType { get; set; }
        public OdTvExtendedView.e3DViewType e3DViewType { get; set; }
        public OdTvGsView.Projection eProjectionType { get; set; }
        public OdTvGsView.RenderMode eRenderModeType { get; set; }
        public OdTvGsDevice.RegenMode eRegenModeType { get; set; }
    }
    public enum OdaEventType
    {
        Panning,
        Orbitting,
        SetZoom,
        Set3DView,
        SetRender,
        SetProjection,
        SetRegen,
        RegenView
    }
}

using Prism.Events;
using Teigha.Visualize;
using HCL_ODA_TestPAD.ViewModels.Base;

namespace HCL_ODA_TestPAD.Mvvm.Events
{
    public class OdaMenuCommandClickEvent : PubSubEvent<OdaMenuCommandClickEventArg>{}

    public record struct OdaMenuCommandClickEventArg
    {
        public OdaEventType OdaEventType { get; init; }
        public ZoomType eZoomType { get; init; }
        public OdTvExtendedView.e3DViewType e3DViewType { get; init; }
        public OdTvGsView.Projection eProjectionType { get; init; }
        public OdTvGsView.RenderMode eRenderModeType { get; init; }
        public OdTvGsDevice.RegenMode eRegenModeType { get; init; }
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

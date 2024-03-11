using Prism.Events;
using HCL_ODA_TestPAD.ViewModels.Base;
using ODA.Visualize.TV_Visualize;
using ODA.Visualize.TV_VisualizeTools;

namespace HCL_ODA_TestPAD.Mvvm.Events
{
    public class OdaMenuCommandClickEvent : PubSubEvent<OdaMenuCommandClickEventArg>{}

    public record struct OdaMenuCommandClickEventArg
    {
        public OdaEventType OdaEventType { get; init; }
        public ZoomType eZoomType { get; init; }
        public OdTvExtendedView_e3DViewType e3DViewType { get; init; }
        public OdTvGsView_Projection eProjectionType { get; init; }
        public OdTvGsView_RenderMode eRenderModeType { get; init; }
        public OdTvGsDevice_RegenMode eRegenModeType { get; init; }
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

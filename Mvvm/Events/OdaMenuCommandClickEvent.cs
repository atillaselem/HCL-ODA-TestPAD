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
        public ZoomType EZoomType { get; init; }
        public OdTvExtendedView_e3DViewType E3DViewType { get; init; }
        public OdTvGsView_Projection EProjectionType { get; init; }
        public OdTvGsView_RenderMode ERenderModeType { get; init; }
        public OdTvGsDevice_RegenMode ERegenModeType { get; init; }
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

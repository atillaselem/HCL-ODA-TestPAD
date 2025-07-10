using ODA.Kernel.TD_RootIntegrated;
using ODA.Visualize.TV_VisualizeTools;
using HCL_ODA_TestPAD.ODA.Draggers.Navigation;
using HCL_ODA_TestPAD.ODA.Draggers;
using HCL_ODA_TestPAD.ViewModels.Base;
using System.Diagnostics;
using System;

namespace HCL_ODA_TestPAD.ViewModels;

public partial class HclCadImageViewModel
{
    public void Zoom(ZoomType type)
    {
        if (TvDatabaseId == null || TvGsDeviceId == null)
            return;

        //VM.UncheckDraggersBtns();

        //MemoryTransaction mtr = MM.StartTransaction();

        OdTvExtendedView exView = GetActiveTvExtendedView();
        if (exView == null)
            return;
        switch (type)
        {
            case ZoomType.ZoomIn:
                exView.zoomIn();
                break;
            case ZoomType.ZoomOut:
                exView.zoomOut();
                break;
            case ZoomType.ZoomExtents:
                {
                    //using var _ = CadProfiler.Log("Time Elapsed", "ZoomExtents");
                    //ServiceFactory.Logger.LogInformation("ZoomExtents");
                    exView.zoomToExtents();
                    //update cached extents if need
                    using var lastExt = new OdGeBoundBlock3d();
                    if (!exView.getCachedExtents(lastExt))
                    {
                        var viewId = exView.getViewId();
                        using var view = viewId.openObject();
                        if (view.getLastViewExtents(lastExt))
                            exView.setViewExtentsForCaching(lastExt);
                    }
                    break;
                }
            default:
                throw new ArgumentOutOfRangeException(type.ToString() + " (" + type.GetType().ToString() + ")", type, null);
        }

        exView.setViewType(OdTvExtendedView_e3DViewType.kCustom);

        if (_dragger != null)
            _dragger.NotifyAboutViewChange(DraggerViewChangeType.ViewChangeZoom);

        UpdateHclZoomTransformations();
        UpdateCadView(true);

        //MM.StopTransaction(mtr);
    }
    public void Pan()
    {
        if (TvDatabaseId == null || TvGsDeviceId == null)
            return;
        ODA.Draggers.OdTvDragger newDragger = new OdTvPanDragger(TvGsDeviceId, _tvDraggersModelId);
        StartDragger(newDragger, true);
    }
    public void Orbit()
    {
        if (TvDatabaseId == null || TvGsDeviceId == null)
            return;

        //             if (_dragger != null && (_dragger as OdTvSelectDragger) == null)
        //                 FinishDragger();

        ODA.Draggers.OdTvDragger newDragger = new OdTvOrbitDragger(TvGsDeviceId, _tvDraggersModelId);
        StartDragger(newDragger, true);
        Debug.WriteLine("1-Orbit");
        //DisableMarkups();
    }

    public void ZoomToArea()
    {
        if (TvDatabaseId == null || TvGsDeviceId == null)
            return;

        //             if (_dragger != null && (_dragger as OdTvSelectDragger) == null)
        //                 FinishDragger();

        ODA.Draggers.OdTvDragger newDragger = new OdTvOrbitDragger(TvGsDeviceId, _tvDraggersModelId);
        StartDragger(newDragger, true);
        Debug.WriteLine("1-Orbit");
        //DisableMarkups();
    }
}
using System.Windows.Media;
using HCL_ODA_TestPAD.HCL.CAD.Math.API;
using ODA.Kernel.TD_RootIntegrated;
using ODA.Visualize.TV_Visualize;
using ODA.Visualize.TV_VisualizeTools;
using HCL_ODA_TestPAD.HCL.Visualize.Extensions;

namespace HCL_ODA_TestPAD.ViewModels;

public partial class HclCadImageViewModel
{
    public CadPoint3D GetViewCenter()
    {
        if (_cachedViewCenter != null)
        {
            return _cachedViewCenter;
        }
        using var odTvGsViewId = GetViewId();
        using var cachedExtents = new OdGeBoundBlock3d();
        using var odTvGsView = odTvGsViewId.openObject(OdTv_OpenMode.kForWrite);
        odTvGsView.getLastViewExtents(cachedExtents);
        return CadPoint3D.With(cachedExtents.center());
    }
    public (CadPoint3D min, CadPoint3D max) GetViewExtent()
    {
        using var odTvGsViewId = GetViewId();
        using var cachedExtents = new OdGeBoundBlock3d();
        using var odTvGsView = odTvGsViewId.openObject();
        odTvGsView.getLastViewExtents(cachedExtents);
        return (cachedExtents.minPoint(), cachedExtents.maxPoint());
    }

    public OdTvGsViewId GetViewId()
    {
        using var odTvGsDevice = TvGsDeviceId.openObject();
        return odTvGsDevice.viewAt(TvActiveViewport);
    }
    public OdTvGsView GetView()
    {
        if (TvGsDeviceId == null) return null;
        using var odTvGsDevice = TvGsDeviceId.openObject();
        using var odTvGsViewId = odTvGsDevice.viewAt(TvActiveViewport);
        return odTvGsViewId.openObject(OdTv_OpenMode.kForRead);
    }
    public bool IsCadViewRotated() => GetViewId().IsCADRotated();
    public void Set3DView(OdTvExtendedView_e3DViewType type)
    {
        //MemoryTransaction mtr = MM.StartTransaction();

        OdTvExtendedView exView = GetActiveTvExtendedView();
        if (exView == null)
            return;

        // set view type
        exView.setViewType(type);

        //update cached extents if need
        using var lastExt = new OdGeBoundBlock3d();
        if (!exView.getCachedExtents(lastExt))
        {
            var viewId = exView.getViewId();
            using var view = viewId.openObject();
            if (view.getLastViewExtents(lastExt))
                exView.setViewExtentsForCaching(lastExt);
        }
        UpdateDefaultModelModelingMatrix();

        var scaleFactor = GetScaleFactor();
        _hclPrism?.ScaleModelAtLocation(scaleFactor, HclPrismLocation);
        _hclPrism?.UpdateViewTransformation();
        _hclStation?.ScaleModelAtLocation(scaleFactor, HclStationLocation);
        _hclStation?.UpdateViewTransformation();
        _hclPointContainer?.UpdateTransformations(GetPointScaleFactor());
        UpdateCadView();

        UpdateWcsView();
        //MM.StopTransaction(mtr);
        if (ServiceFactory.AppSettings.ShowCube)
        {
            OnOffViewCube(true);
        }
    }

    private void UpdateWcsView()
    {
        using var parentView = Wcs.GetParentView(OdTv_OpenMode.kForRead);
        var parentViewPosition = parentView.position();
        var targetTranslation = OdGeMatrix3d.translation(-parentView.target().asVector());

        using var wcsView = Wcs.GetWcsView(OdTv_OpenMode.kForWrite);

        wcsView.setView(parentViewPosition.transformBy(targetTranslation),
                            OdGePoint3d.kOrigin, parentView.upVector(), 1, 1);
        wcsView.setMode(OdTvGsView_RenderMode.kGouraudShaded);
        wcsView.zoom(4.2);
        OdGePoint2d lowerLeft = new OdGePoint2d();
        OdGePoint2d upperRight = new OdGePoint2d();
        parentView.getViewport(lowerLeft, upperRight);
        upperRight.x = lowerLeft.x + 1.9;
        upperRight.y = lowerLeft.y + 0.23;
        wcsView.setViewport(lowerLeft, upperRight);
        UpdateCadView();
    }

    public OdGeVector3d GetEyeDirection()
    {
        OdGeVector3d eyeDir = new OdGeVector3d(OdGeVector3d.kIdentity);
        MemoryTransaction mtr = _mm.StartTransaction();

        OdTvGsView pView = GetActiveTvExtendedView().getViewId().openObject();
        if (pView != null)
            eyeDir = pView.position() - pView.target();

        _mm.StopTransaction(mtr);
        return eyeDir;
    }
    public void SetRenderMode(OdTvGsView_RenderMode renderMode)
    {
        //MemoryTransaction mtr = MM.StartTransaction();
        if (TvGsDeviceId != null && !TvGsDeviceId.isNull())
        {
            using var odTvGsDevice = TvGsDeviceId.openObject();
            using var view = odTvGsDevice.viewAt(TvActiveViewport).openObject(OdTv_OpenMode.kForWrite);
            OdTvGsView_RenderMode oldMode = view.mode();
            if (oldMode != renderMode)
            {
                view.setMode(renderMode);

                // set mode for WCS
                //if (WCS != null && ServiceFactory.AppSettings.ShowWCS && WCS.IsNeedUpdateWCS(oldMode, renderMode))
                //    WCS.UpdateWCS();

                //TvDeviceId.openObject().update();
                UpdateCadView();
                ViewControl.SetRenderModeButton(renderMode);
            }
        }
        //MM.StopTransaction(mtr);
    }
    public void SetProjectionType(OdTvGsView_Projection projection)
    {
        MemoryTransaction mtr = _mm.StartTransaction();
        if (TvGsDeviceId != null && !TvGsDeviceId.isNull())
        {
            OdTvGsView view = TvGsDeviceId.openObject().viewAt(TvActiveViewport).openObject(OdTv_OpenMode.kForWrite);
            view.setView(view.position(), view.target(), view.upVector(), view.fieldWidth(), view.fieldHeight(), projection);
            //TvDeviceId.openObject().update();
            UpdateCadView();
        }
        _mm.StopTransaction(mtr);
    }

    public void SetBackgroundColor(Color color)
    {
        MemoryTransaction mtr = _mm.StartTransaction();
        uint iColor = ((uint)(color.R | color.G << 8 | ((color.B) << 16)));
        if (TvGsDeviceId != null && !TvGsDeviceId.isNull())
        {
            OdTvGsDevice dev = TvGsDeviceId.openObject(OdTv_OpenMode.kForWrite);
            dev.setBackgroundColor(iColor);
            dev.update();
        }
        _mm.StopTransaction(mtr);
    }
    private void UpdateDefaultModelModelingMatrix()
    {
        if (TvDefaultModelId is null)
        {
            return;
        }
        //Find Entity under parent Model
        using var modelObject = TvDefaultModelId.openObject(OdTv_OpenMode.kForWrite);
        var foundEntityId = modelObject.findEntity(InnerRectangleEntityHandle);

        //Get Inverse Camera Matrix
        using var odTvGsDevice = TvGsDeviceId.openObject();
        using var odTvGsViewId = odTvGsDevice.viewAt(TvActiveViewport);
        using var odTvGsView = odTvGsViewId.openObject(OdTv_OpenMode.kForWrite);
        using var xEyeToWorld = odTvGsView.eyeToWorldMatrix();
        //Set rotation matrix for the polygon Entity which should be facing camera
        modelObject.setModelingMatrix(foundEntityId, xEyeToWorld.setTranslation(new OdGeVector3d(0, 0, 0)), true);
    }
}
using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using HCL_ODA_TestPAD.HCL;
using HCL_ODA_TestPAD.HCL.CAD.Math.API;
using HCL_ODA_TestPAD.HCL.Tools;
using HCL_ODA_TestPAD.HCL.Visualize;
using HCL_ODA_TestPAD.HCL.Visualize.Extensions;
using HCL_ODA_TestPAD.Mvvm.Events;
using HCL_ODA_TestPAD.Utility;
using HCL_ODA_TestPAD.ViewModels.Base;
using ODA.Kernel.TD_RootIntegrated;
using ODA.Visualize.TV_Visualize;
using ODA.Visualize.TV_VisualizeTools;

namespace HCL_ODA_TestPAD.ViewModels;

public partial class HclCadImageViewModel : IHclTooling
{
    private CadPoint3D _cachedViewCenter;
    public CadPoint3D HclPrismLocation { get; set; }
    public CadPoint3D HclStationLocation { get; set; }
    public double LastRadius { get; set; } = 1;
    public double PointScaleFactor { get; set; } = 1;
    public double PrismScaleFactor { get; set; } = 1;
    private HclStation _hclStation;
    private HclPrism _hclPrism;

    private HclPointContainer _hclPointContainer;
    private CadLine _hclToolLine;
    public bool PointScaleFactorSet { get; set; }
    public bool PrismScaleFactorSet { get; set; }
    private int _pointMoveCycle = default;
    public OdTvModel GetTopModel()
    {
        using var odTvDatabase = TvDatabaseId.openObject();
        using var modelsIterator = odTvDatabase.getModelsIterator();
        for (; !modelsIterator.done(); modelsIterator.step())
        {
            TvTopModelId = modelsIterator.getModel();
            var odTvModel = TvTopModelId.openObject(OdTv_OpenMode.kForRead);
            if (!TvTopModelId.isNull())
            {
                return odTvModel;
            }
        }
        throw new InvalidOperationException("No model found in the database");
    }
    private void OnPrismTypeChanged(string prismType)
    {
        HclPrismBuilder.Dispose(this);
        ShowTool(HclToolType.Prism);
        Zoom(ZoomType.ZoomIn);
        Zoom(ZoomType.ZoomOut);
    }
    public void ShowTool(HclToolType type)
    {
        _cachedViewCenter = GetViewCenter();
        HclStationLocation ??= CadPoint3D.With(_cachedViewCenter);
        HclPrismLocation ??= CadPoint3D.With(_cachedViewCenter);
        using var viewPos = ViewParameters.FromView(this);
        switch (type)
        {
            case HclToolType.PLTStation:
                _hclStation = HclStationBuilder.ShowPltStation(this, HclStationLocation);
                _hclStation?.UpdateViewTransformation();
                break;
            case HclToolType.Prism:
                _hclPrism = HclPrismBuilder.ShowPrism(this, HclPrismLocation);
                _hclPrism?.UpdateViewTransformation();
                break;
            case HclToolType.Points:
                if (_hclPointContainer == null)
                {
                    Set3DView(OdTvExtendedView_e3DViewType.kTop);
                }
                _hclPointContainer = HclPointBuilder.ShowPoints(this);
                _hclPointContainer?.UpdateTransformations(GetPointScaleFactor());
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
        UpdateToolLine();
        UpdateCadView();
    }

    public void ChangePrismLocation(Point mousePosition)
    {
        if (_hclPrism == null) return;
        using var oldLocation = CadPoint3D.With(HclPrismLocation!);
        HclPrismLocation?.Dispose();
        HclPrismLocation = UpdateStatusBarCoordinates(mousePosition);
        _hclPrism?.UpdateLocationDelta(HclPrismLocation, oldLocation);
        UpdateToolLine();
        UpdateCadView();
    }
    public void ChangeStationLocation(Point mousePosition)
    {
        if (_hclStation == null) return;
        using var oldLocation = CadPoint3D.With(HclStationLocation!);
        HclStationLocation?.Dispose();
        HclStationLocation = UpdateStatusBarCoordinates(mousePosition);
        _hclStation?.UpdateLocationDelta(HclStationLocation, oldLocation);
        UpdateToolLine();
        UpdateCadView();
    }

    private void UpdateToolLine()
    {
        if (_hclToolLine is not null && _hclToolLine.TvModelId != null)
        {
            _hclToolLine.Remove(this);
            _hclToolLine = null;
        }
        if (_hclStation != null && _hclPrism != null)
        {
            _hclToolLine = HclToolLineBuilder.ShowToolLine(this, HclStationLocation, HclPrismLocation);
        }
        if (_hclToolLine != null)
        {
            var geometryDataId = _hclToolLine?.VisibleEntityGeometryDict.Values.ToList()[0];
            using var polyLine = geometryDataId.openAsPolyline();
            using var points = new OdGePoint3dVector();
            polyLine.getPoints(points);
            var message = $"Tool Line Wcs: Point 1 = [{points[0].x:0.##} - {points[0].y:0.##} - {points[0].z:0.##}];Tool Line Wcs: Point 2 = [{points[1].x:0.##} - {points[1].y:0.##} - {points[1].z:0.##}]";
            ServiceFactory.EventSrv.GetEvent<ScreenCoordinatesChanged>().Publish(message);
        }
    }

    public CadPoint3D UpdateStatusBarCoordinates(Point mousePosition)
    {
        var screenLocation = CadScreenInfoProvider.DpiScaledPoint(mousePosition);
        var mouseWcsLocation = screenLocation.FromScreenToWorld(GetViewId());
        var message = $"Screen Coordinates:[{mouseWcsLocation.X:#.##} - {mouseWcsLocation.Y:#.##}];World Coordinates:[{mouseWcsLocation.X:0.##} - {mouseWcsLocation.Y:0.##} - {mouseWcsLocation.Z:0.##}]";
        ServiceFactory.EventSrv.GetEvent<ScreenCoordinatesChanged>().Publish(message);
        return mouseWcsLocation;
    }

    public double GetScaleFactor()
    {
        using var viewId = GetViewId();
        var radius = viewId.GetPixelScaleFactorAtViewTarget(100);
        var scaleFactor = radius / LastRadius;
        LastRadius = radius;

        var message = $"CR:[{radius:0.##} - LR:[{LastRadius:0.##}] - Scale Factor:[{scaleFactor:0.##}]";
        ServiceFactory.EventSrv.GetEvent<ScaleFactorChanged>().Publish(message);
        return scaleFactor;
    }
    public double GetPointScaleFactor()
    {
        using var viewId = GetViewId();
        var radius = viewId.GetPixelScaleFactorAtViewTarget(100);
        var scaleFactor = radius / PointScaleFactor;

        if (_hclPointContainer != null && !PointScaleFactorSet)
        {
            PointScaleFactorSet = true;
            PointScaleFactor = scaleFactor;
        }
        return scaleFactor;
    }

    public void UpdateHclZoomTransformations(bool isPinchZoom = false)
    {
        if (!_isFileLoading)
        {
            return;
        }
        var scaleFactor = GetScaleFactor();
        _hclStation?.ScaleModelAtLocation(scaleFactor, HclStationLocation);
        _hclPrism?.ScaleModelAtLocation(scaleFactor, HclPrismLocation);
        if (isPinchZoom)
        {
            OptimizePointTransformations();
        }
        else
        {
            _hclPointContainer?.UpdateTransformations(GetPointScaleFactor());
        }
    }
    private void OptimizePointTransformations()
    {
        if (_hclPointContainer == null)
        {
            return;
        }
        if (ServiceFactory.AppSettings.EnablePointOptimization)
        {
            _pointMoveCycle++;
            if (ServiceFactory.AppSettings.UseDynamicOptimization)
            {
                var countOfPointCycle = HclPointContainer.PointListCount / 1000 + 1;
                if ((_pointMoveCycle % countOfPointCycle) == 0)
                {
                    DoPointTransformation();
                }
            }
            else if (ServiceFactory.AppSettings.RenderEveryXCycle == 0)
            {
                return;
            }
            else if ((_pointMoveCycle % ServiceFactory.AppSettings.RenderEveryXCycle) == 0)
            {
                // Do fixed cycle point transformation
                DoPointTransformation();
            }
        }
        else
        {
            DoPointTransformation();
        }
    }
    private void DoPointTransformation()
    {
        _pointMoveCycle = 0;
        _hclPointContainer?.UpdateTransformations(GetPointScaleFactor());
    }
    internal void ToggleTextTransformation(bool showText)
    {
        _hclPointContainer?.TogglePointText(showText);
        UpdateCadView();
    }

    #region Fill Color
    internal void ApplyFillColor()
    {
        OverrideEntityColor();
        UpdateCadView();
    }
    private void OverrideEntityColor()
    {
        using var defaultModel = GetTopModel();
        HiltiCadConstants.DefaultModelName = defaultModel.getName();
        using var defaultIt = defaultModel.getEntitiesIterator();
        var settingsColor = ResourcesResolver.ResolveResource<Color>(ServiceFactory.AppSettings.FillColor.ToString())
            .ToDrawingColor();

        using var color = new OdTvColorDef(settingsColor.R, settingsColor.G, settingsColor.B);
        while (!defaultIt.done())
        {
            using var entity = defaultIt.getEntity();
            defaultIt.step();
            if (entity is null || entity.isNull() || entity.getType() != OdTvEntityId_EntityTypes.kEntity)
            {
                continue;
            }
            using var entityOpen = entity.openObject(OdTv_OpenMode.kForRead);
            entityOpen.setColor(color);
            //for ifc file, apply color to also sub-entity
            if (_isIfc)
            {
                entityOpen.OverrideSubEntityColor(color);
            }
        }
    }
    #endregion
}
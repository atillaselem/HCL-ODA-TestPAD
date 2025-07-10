using HCL_ODA_TestPAD.ViewModels.Base;
using System.Windows;
using ODA.Visualize.TV_Visualize;
using HCL_ODA_TestPAD.HCL.Profiler;
using HCL_ODA_TestPAD.HCL.Tools;
using HCL_ODA_TestPAD.HCL.UserActions.States;
using HCL_ODA_TestPAD.ODA.Draggers.Navigation;
using HCL_ODA_TestPAD.ODA.Draggers;
using System.Windows.Input;

namespace HCL_ODA_TestPAD.ViewModels;

public partial class HclCadImageViewModel
{
    private MouseButtonState _mouseDown = MouseButtonState.Released;

    public void MouseDown(MouseButtonEventArgs e, Point position)
    {
        if (_dragger is OdTvOrbitDragger)
        {
            OnOffViewCube(false);
        }
        _mouseDown = e.MouseDevice.LeftButton == MouseButtonState.Pressed ? MouseButtonState.Pressed :
            e.MouseDevice.RightButton == MouseButtonState.Pressed ? MouseButtonState.Pressed : MouseButtonState.Released;

        if (_dragger == null) return;

        // activation first
        DraggerResult res = _dragger.Activate();
        ActionAfterDragger(res);
        res = _dragger.NextPoint((int)position.X, (int)position.Y);
        ActionAfterDragger(res);
        if (ServiceFactory.AppSettings.Interactivity)
        {
            //start Interactivity
            using var odTvGsDevice = TvGsDeviceId.openObject();
            using var odTvGsView = odTvGsDevice.viewAt(TvActiveViewport).openObject(OdTv_OpenMode.kForWrite);
            odTvGsView.beginInteractivity(ServiceFactory.AppSettings.InteractiveFps);
        }
    }

    public void MouseMove(MouseEventArgs e, Point position, UserInteraction userInteraction)
    {
        if (_dragger != null)
        {
            if (_mouseDown == MouseButtonState.Pressed)
            {
                DraggerResult res = _dragger.Drag((int)position.X, (int)position.Y);
                //Debug.WriteLine("4-AtvViewer-MouseMoveEvent");
                if (userInteraction == UserInteraction.Orbiting)
                {
                    UpdateDefaultModelModelingMatrix();
                    //OK : Arrow rotates properly 
                    _hclPrism?.UpdateViewTransformation();
                    _hclStation?.UpdateViewTransformation();
                    using var _ = CadProfiler.LogFps($"Point Tx - # of Points {HclPointContainer.PointListCount}", "Mouse/Touch Move - Orbiting");
                    OptimizePointTransformations();
                    UpdateCadView();
                }
                else
                {
                    ActionAfterDragger(res);
                }
            }
        }
    }
    public void MouseUp(MouseButtonEventArgs e)
    {
        _mouseDown = MouseButtonState.Released;
        if (ServiceFactory.AppSettings.Interactivity)
        {
            //start Interactivity
            using var odTvGsDevice = TvGsDeviceId.openObject();
            using var odTvGsView = odTvGsDevice.viewAt(TvActiveViewport).openObject(OdTv_OpenMode.kForWrite);
            odTvGsView.endInteractivity();
        }
        UpdateCadView();
    }

    public void MouseWheel(MouseWheelEventArgs e)
    {
        bool zoomIn = e.Delta > 0;
        try
        {
            Zoom(zoomIn ? ZoomType.ZoomIn : ZoomType.ZoomOut);
        }
        finally
        {
        }
    }
}
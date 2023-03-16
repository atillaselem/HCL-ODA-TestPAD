using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using HCL_ODA_TestPAD.ViewModels.Base;
using Teigha.Core;
using Teigha.Visualize;

namespace HCL_ODA_TestPAD.HCL.MouseTouch
{
    public class ZoomToAreaManager
    {
        /// <summary>
        /// Set to 'true' when the left mouse-button is down.
        /// </summary>
        private bool _isLeftMouseButtonDownOnWindow;

        /// <summary>
        /// Set to 'true' when dragging the 'selection rectangle'.
        /// Dragging of the selection rectangle only starts when the left mouse-button is held down and the mouse-cursor
        /// is moved more than a threshold distance.
        /// </summary>
        private bool _isDraggingSelectionRect;

        /// <summary>
        /// Records the location of the mouse (relative to the window) when the left-mouse button has pressed down.
        /// </summary>
        private Point _originalMouseDownPoint;

        private readonly List<int> _arrTouches = new();

        public bool IsZoomToAreaEnabled { get; set; }

        private ZoomHelper.OperationMode _operationMode = ZoomHelper.OperationMode.None;

        private readonly ICadImageViewControl _viewControl;
        private readonly Func<Canvas> _canvasFactory;
        private readonly Func<Border> _borderFactory;

        private readonly OdGePoint3dArray _zoomAreaPoints = new();
        private readonly CadTransformer _cadTransformer;
        private readonly CadZoomOperations _cadZoomOperations;

        public ZoomToAreaManager(ICadImageViewControl viewControl, Func<Canvas> canvasFactory, Func<Border> borderFactory, OdTvGsDeviceId odTvGsDeviceId)
        {
            _viewControl = viewControl;
            _canvasFactory = canvasFactory;
            _borderFactory = borderFactory;
            using var odTvGsDevice = odTvGsDeviceId.openObject(OpenMode.kForRead);
            _cadTransformer = new CadTransformer(odTvGsDevice.getActiveView());
            _cadZoomOperations = new CadZoomOperations(odTvGsDevice.getActiveView());
        }

        private static Point GetCadPoint(Point wpfLocation)
        {
            return new Point(wpfLocation.X * ContentScaleFactor,
                wpfLocation.Y * ContentScaleFactor);
        }
        private static double _contentScaleFactor;
        public static double ContentScaleFactor
        {
            get
            {
                if (_contentScaleFactor != 0) return _contentScaleFactor;
                new CadScreenInfoProvider().GetEffectiveDpi(out var dpiX, out _);
                _contentScaleFactor = dpiX / 96.0;
                return _contentScaleFactor;
            }
        }
        public void HandleTouchAndMouseDown<T>(T e, UserControl window) where T : InputEventArgs
        {

            if (IsZoomToAreaEnabled)
            {
                int deviceId = e.GetDeviceId();
                _arrTouches.Add(deviceId);
                var wpfLocation = e.GetPosition(window);
                var location = GetCadPoint(wpfLocation);

                _originalMouseDownPoint = wpfLocation;
                _isLeftMouseButtonDownOnWindow = true;
                _zoomAreaPoints.Clear();
                _zoomAreaPoints.Add(_cadTransformer.GetWorldCoordinates(location.X, location.Y));
                _operationMode = ZoomHelper.OperationMode.ZoomToArea;
            }
        }
        public void HandleTouchAndMouseMove<T>(T e, UserControl window) where T : InputEventArgs
        {
            if (_operationMode == ZoomHelper.OperationMode.ZoomToArea)
            {
                int mouseMoveDeviceId = e.GetDeviceId();
                var wpfLocation = e.GetPosition(window);

                if (_arrTouches.IndexOf(mouseMoveDeviceId) == 0)
                {
                    if (_isDraggingSelectionRect)
                    {
                        // Drag selection is in progress.
                        Point curMouseDownPoint = wpfLocation;
                        UpdateDragSelectionRect(_originalMouseDownPoint, curMouseDownPoint);
                    }
                    else if (_isLeftMouseButtonDownOnWindow)
                    {
                        // The user is left-dragging the mouse,
                        // but don't initiate drag selection until
                        // they have dragged past the threshold value.
                        Point curMouseDownPoint = wpfLocation;
                        var dragDelta = curMouseDownPoint - _originalMouseDownPoint;
                        double dragDistance = Math.Abs(dragDelta.Length);
                        if (dragDistance > CADModelConstants.DragThreshold)
                        {
                            // When the mouse has been dragged more than the threshold value commence drag selection.
                            _isDraggingSelectionRect = true;
                            InitDragSelectionRect(_originalMouseDownPoint, curMouseDownPoint);
                        }
                    }
                }
            }
        }

        public void HandleTouchAndMouseUp<T>(T e, UserControl window) where T : InputEventArgs
        {
            if (_operationMode == ZoomHelper.OperationMode.ZoomToArea)
            {
                int mouseMoveDeviceId = e.GetDeviceId();
                var wpfLocation = e.GetPosition(window);
                var location = GetCadPoint(wpfLocation);

                if (_arrTouches.IndexOf(mouseMoveDeviceId) == 0)
                {
                    _isLeftMouseButtonDownOnWindow = false;
                    _isDraggingSelectionRect = false;
                    _canvasFactory().Visibility = Visibility.Hidden;

                    try
                    {
                        _zoomAreaPoints.Add(_cadTransformer.GetWorldCoordinates(location.X, location.Y));
                        if (_zoomAreaPoints.Count != 2)
                        {
                            return;
                        }

                        _cadZoomOperations.ZoomToArea(_zoomAreaPoints[0], _zoomAreaPoints[1]);
                        _viewControl.UpdateView();
                        _zoomAreaPoints.Clear();
                    }
                    catch 
                    {
                    }
                    ResetTouchFlags();
                }
            }

        }

        private void ResetTouchFlags()
        {
            _operationMode = ZoomHelper.OperationMode.None;
            _arrTouches.Clear();
        }

        /// <summary>
        /// Update the position and size of the rectangle used for drag selection.
        /// </summary>
        private void UpdateDragSelectionRect(Point pt1, Point pt2)
        {
            double x, y, width, height;

            // Determine x,y,width and height of the rect inverting the points if necessary.
            if (pt2.X < pt1.X)
            {
                x = pt2.X;
                width = pt1.X - pt2.X;
            }
            else
            {
                x = pt1.X;
                width = pt2.X - pt1.X;
            }

            if (pt2.Y < pt1.Y)
            {
                y = pt2.Y;
                height = pt1.Y - pt2.Y;
            }
            else
            {
                y = pt1.Y;
                height = pt2.Y - pt1.Y;
            }

            // Update the coordinates of the rectangle used for drag selection.
            Canvas.SetLeft(_borderFactory(), x);
            Canvas.SetTop(_borderFactory(), y);
            _borderFactory().Width = width;
            _borderFactory().Height = height;
        }

        /// <summary>
        /// Initialize the rectangle used for drag selection.
        /// </summary>
        private void InitDragSelectionRect(Point pt1, Point pt2)
        {
            UpdateDragSelectionRect(pt1, pt2);
            _canvasFactory().Visibility = Visibility.Visible;
        }

    }
}

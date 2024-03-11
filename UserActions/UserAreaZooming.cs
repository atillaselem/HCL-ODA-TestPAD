// Copyright © 2018 by Hilti Corporation – all rights reserved

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using HCL_ODA_TestPAD.HCL;
using HCL_ODA_TestPAD.HCL.MouseTouch;
using HCL_ODA_TestPAD.Settings;
using HCL_ODA_TestPAD.ViewModels.Base;
using ODA.Kernel.TD_RootIntegrated;
using ODA.Visualize.TV_Visualize;

namespace HCL_ODA_TestPAD.UserActions
{
    public class UserAreaZooming : UserActionBase
    {
        private bool _isLeftMouseButtonDownOnWindow;

        private bool _isDraggingSelectionRect;

        private Point _originalMouseDownPoint;

        private readonly Func<Canvas> _canvasFactory;
        private readonly Func<Border> _borderFactory;

        private readonly OdGePoint3dArray _zoomAreaPoints = new();
        private readonly CadTransformer _cadTransformer;
        private readonly CadZoomOperations _cadZoomOperations;
        private readonly ICadImageViewControl _viewControl;
        private readonly OdTvGsViewId _odTvGsViewId;
        private readonly AppSettings _appSettings;

        public UserAreaZooming(ICadImageViewControl viewControl,
            OdTvGsDeviceId odTvGsDeviceId, 
            AppSettings appSettings,
            Func<Canvas> canvasFactory, Func<Border> borderFactory)
        {
            _viewControl = viewControl;
            _appSettings = appSettings;
            _canvasFactory = canvasFactory;
            _borderFactory = borderFactory;
            using var odTvGsDevice = odTvGsDeviceId.openObject(OdTv_OpenMode.kForRead);
            _odTvGsViewId = odTvGsDevice.getActiveView();
            _cadZoomOperations = new CadZoomOperations(_odTvGsViewId);
            _cadTransformer = new CadTransformer(odTvGsDevice.getActiveView());
            _cadZoomOperations = new CadZoomOperations(odTvGsDevice.getActiveView());
        }

        public void HandleMouseTouchDown<T>(T e, UserControl window) where T : InputEventArgs
        {
            var deviceId = e.GetDeviceId();
            TouchPoints.Add(deviceId);

            var wpfLocation = e.GetPosition(window);
            var location = DpiScaledMousePosition(e, window);

            _originalMouseDownPoint = wpfLocation;
            _isLeftMouseButtonDownOnWindow = true;
            _zoomAreaPoints.Clear();
            _zoomAreaPoints.Add(_cadTransformer.GetWorldCoordinates(location.X, location.Y));
        }
        public void HandleMouseTouchMove<T>(T e, UserControl window) where T : InputEventArgs
        {
            int mouseMoveDeviceId = e.GetDeviceId();
            var wpfLocation = e.GetPosition(window);

            if (TouchPoints.IndexOf(mouseMoveDeviceId) == 0)
            {
                if (_isDraggingSelectionRect)
                {
                    // Drag selection is in progress.
                    UpdateDragSelectionRect(_originalMouseDownPoint, wpfLocation);
                }
                else if (_isLeftMouseButtonDownOnWindow)
                {
                    // The user is left-dragging the mouse,
                    // but don't initiate drag selection until
                    // they have dragged past the threshold value.
                    var dragDelta = wpfLocation - _originalMouseDownPoint;
                    var dragDistance = Math.Abs(dragDelta.Length);

                    if (!(dragDistance > CADModelConstants.DragThreshold))
                    {
                        return;
                    }

                    // When the mouse has been dragged more than the threshold value commence drag selection.
                    _isDraggingSelectionRect = true;
                    InitDragSelectionRect(_originalMouseDownPoint, wpfLocation);
                }
            }
        }

        public void HandleMouseTouchUp<T>(T e, UserControl window) where T : InputEventArgs
        {
            var mouseMoveDeviceId = e.GetDeviceId();
            var location = DpiScaledMousePosition(e, window);

            if (TouchPoints.IndexOf(mouseMoveDeviceId) != 0)
            {
                return;
            }

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
            }
            catch
            {
                // ignored
            }
            finally
            {
                ResetTouchFlags();
            }

        }

        /// <summary>
        /// Update the position and size of the rectangle used for drag selection.
        /// </summary>
        private void UpdateDragSelectionRect(Point pt1, Point pt2)
        {
            // Determine x,y,width and height of the rect.
            var x = Math.Min(pt1.X, pt2.X);
            var width = Math.Abs(pt1.X - pt2.X);

            var y = Math.Min(pt1.Y, pt2.Y);
            var height = Math.Abs(pt1.Y - pt2.Y);

            // Update the coordinates of the rectangle used for drag selection.
            Canvas.SetLeft(_borderFactory(), x);
            Canvas.SetTop(_borderFactory(), y);
            _borderFactory().Width = width;
            _borderFactory().Height = height;
        }

        private void InitDragSelectionRect(Point pt1, Point pt2)
        {
            UpdateDragSelectionRect(pt1, pt2);
            _canvasFactory().Visibility = Visibility.Visible;
        }
    }

}

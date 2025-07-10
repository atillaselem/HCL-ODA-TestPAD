// Copyright © 2018 by Hilti Corporation – all rights reserved

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HCL_ODA_TestPAD.HCL.UserActions
{
    public class UserSelectionWindow : UserActionBase
    {
        private bool _isLeftMouseButtonDownOnWindow;

        private bool _isDraggingSelectionRect;

        private Point _originalMouseDownPoint;

        private readonly Func<Canvas> _canvasFactory;
        private readonly Func<Border> _borderFactory;
        public List<Point> SelectionWindowScreenPoints; 
        public bool IsEnabled { get; set; }


        public UserSelectionWindow(Func<Canvas> canvasFactory, Func<Border> borderFactory)
        {
            _canvasFactory = canvasFactory;
            _borderFactory = borderFactory;
            SelectionWindowScreenPoints = new List<Point>();
        }

        public void HandleMouseTouchDown<T>(T e, UserControl window) where T : InputEventArgs
        {
            var deviceId = e.GetDeviceId();
            TouchPoints.Add(deviceId);

            var wpfLocation = e.GetPosition(window);
 
            SelectionWindowScreenPoints.Add(wpfLocation);
            _originalMouseDownPoint = wpfLocation;
            _isLeftMouseButtonDownOnWindow = true;
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

                    if (!(dragDistance > CadModelConstants.DragThreshold))
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

            if (TouchPoints.IndexOf(mouseMoveDeviceId) != 0)
            {
                return;
            }

            var wpfLocation = e.GetPosition(window);
            SelectionWindowScreenPoints.Add(wpfLocation);

            _isLeftMouseButtonDownOnWindow = false;
            _isDraggingSelectionRect = false;
            _canvasFactory().Visibility = Visibility.Hidden;

            try
            {
                if (SelectionWindowScreenPoints.Count != 2)
                {
                    return;
                }

                ResetTouchFlags();
            }
            catch
            {
                // ignored
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

        public void ClearSelectionPoints()
        {
            SelectionWindowScreenPoints.Clear();
        }
    }

}

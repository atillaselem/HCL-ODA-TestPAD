using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using Teigha.Core;
using HCL_ODA_TestPAD.HCL.Exceptions;
using HCL_ODA_TestPAD.ViewModels.Base;
using Teigha.Visualize;
using HCL_ODA_TestPAD.Settings;

namespace HCL_ODA_TestPAD.HCL.MouseTouch
{
    public static class InputEventArgsExtensions
    {
        public static Point GetPosition<T>(this T e, IInputElement obj)
            where T : InputEventArgs
        {
            if (e is MouseEventArgs)
                return (e as MouseEventArgs).GetPosition(obj);
            else if (e is TouchEventArgs)
                return (e as TouchEventArgs).GetTouchPoint(obj).Position;

            return new Point();
        }
        public static int GetDeviceId<T>(this T e)
            where T : InputEventArgs
        {
            if (e is MouseEventArgs)
                return (e as MouseEventArgs).Device.GetHashCode();
            else if (e is TouchEventArgs)
                return (e as TouchEventArgs).TouchDevice.Id;
            return 0;
        }
    }
    public class ZoomToScaleManager
    {
        private readonly ICadImageViewControl _viewControl;

        private readonly List<int> _arrTouches = new();

        private Point? _firstOfTwoPoints;
        private Point? _secondOfTwoPoints;

        private double _previousMaximumDist = -1;

        #region Operation mode Enum

        private ZoomHelper.OperationMode _operationMode = ZoomHelper.OperationMode.None;
        private int _touchDeviceId;
        #endregion


        private readonly CadZoomOperations _cadZoomOperations;
        private readonly OdTvGsViewId _odTvGsViewId;
        private readonly AppSettings _appSettings;
        public ZoomToScaleManager(ICadImageViewControl viewControl, OdTvGsDeviceId odTvGsDeviceId, AppSettings appSettings)
        {
            _viewControl = viewControl;
            using var odTvGsDevice = odTvGsDeviceId.openObject(OpenMode.kForRead);
            _odTvGsViewId = odTvGsDevice.getActiveView();
            _cadZoomOperations = new CadZoomOperations(_odTvGsViewId);
            _appSettings = appSettings;
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
        public bool HandleTouchAndMouseDown<T>(T e, UserControl window) where T : InputEventArgs
        {
            _touchDeviceId = e.GetDeviceId();
            var wpfLocation = e.GetPosition(window);
            var location = GetCadPoint(wpfLocation);
            int deviceId = _touchDeviceId;

            _arrTouches.Add(deviceId);
            if (_arrTouches.Count == 1)
            {
                if (_arrTouches.IndexOf(deviceId) == 0)
                {
                    _firstOfTwoPoints = location;
                }
            }
            else if (_arrTouches.Count == 2)
            {

                if (_arrTouches.IndexOf(deviceId) == 1)
                {
                    _secondOfTwoPoints = location;
                }
                if (_firstOfTwoPoints == null || _secondOfTwoPoints == null)
                {
                    return false;
                }

                //Calculate Initial distance between touch points
                _previousMaximumDist = ZoomHelper.CalculateDistance(_firstOfTwoPoints.Value, _secondOfTwoPoints.Value);
                _operationMode = ZoomHelper.OperationMode.ZoomToScale;
                return true;
            }
            if (_appSettings.Interactivity)
            {
                //start Interactivity
                using var odTvGsView = _odTvGsViewId.openObject(OpenMode.kForRead);
                odTvGsView.beginInteractivity(_appSettings.InteractiveFPS);
            }
            return false;
        }
        public void HandleTouchAndMouseMove<T>(T e, UserControl window) where T : InputEventArgs
        {
            if (_operationMode == ZoomHelper.OperationMode.ZoomToScale)
            {
                int touchDeviceId = e.GetDeviceId();
                var wpfLocation = e.GetPosition(window);
                var touchLocation = GetCadPoint(wpfLocation);

                if (_arrTouches.IndexOf(touchDeviceId) == 0)
                {
                    _firstOfTwoPoints = touchLocation;
                }
                else if (_arrTouches.IndexOf(touchDeviceId) == 1)
                {
                    _secondOfTwoPoints = touchLocation;
                }
                if (_firstOfTwoPoints == null || _secondOfTwoPoints == null)
                {
                    return;
                }

                double newDist = -1;

                if (_arrTouches.Count == 2)
                {
                    //Calculate new distance on Move
                    newDist = ZoomHelper.CalculateDistance(_firstOfTwoPoints.Value, _secondOfTwoPoints.Value);
                }
                if (Math.Abs(_previousMaximumDist - newDist) > CADModelConstants.ZoomThresholdDistance)
                {
                    //Calculate the ZoomToScale scale from new and old distance
                    double scale = newDist / _previousMaximumDist;
                    _previousMaximumDist = newDist;

                    try
                    {
                        _cadZoomOperations.Zoom(scale);
                        _viewControl.UpdateView();
                    }
                    catch (ZoomNotPossibleException)
                    {

                    }
                }
            }
        }

        public void HandleTouchAndMouseUp<T>(T e, UserControl window) where T : InputEventArgs
        {

            int touchDeviceId = e.GetDeviceId();

            if (_arrTouches.Contains(touchDeviceId))
            {
                if (touchDeviceId == _arrTouches[0])
                {
                    _firstOfTwoPoints = null;
                }
                else if (touchDeviceId == _arrTouches[1])
                {
                    _secondOfTwoPoints = null;
                }
                _arrTouches.Remove(touchDeviceId);
            }
            _previousMaximumDist = -1;

            if (_arrTouches.Count == 0)
            {
                _operationMode = ZoomHelper.OperationMode.None;
            }

            ResetTouchFlags();
        }

        private void ResetTouchFlags()
        {
            _arrTouches.Clear();
            _firstOfTwoPoints = null;
            _secondOfTwoPoints = null;
            //end Interactivity
            if (_appSettings.Interactivity)
            {
                using var odTvGsView = _odTvGsViewId.openObject(OpenMode.kForRead);
                odTvGsView.endInteractivity();
            }
        }

    }
}

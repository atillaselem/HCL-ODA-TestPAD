// Copyright © 2018 by Hilti Corporation – all rights reserved

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using HCL_ODA_TestPAD.HCL.Exceptions;
using HCL_ODA_TestPAD.HCL.MouseTouch;
using HCL_ODA_TestPAD.Settings;
using HCL_ODA_TestPAD.ViewModels.Base;
using ODA.Visualize.TV_Visualize;

namespace HCL_ODA_TestPAD.HCL.UserActions
{
    public class UserPinchZooming : UserActionBase
    {
        private readonly ICadImageViewControl _viewControl;
        private readonly List<int> _arrTouches = new();

        private Point? _firstOfTwoPoints;
        private Point? _secondOfTwoPoints;

        private double _previousMaximumDist = -1;

        private readonly CadZoomOperations _cadZoomOperations;
        private readonly OdTvGsViewId _odTvGsViewId;
        private readonly AppSettings _appSettings;

        public UserPinchZooming(ICadImageViewControl viewControl, OdTvGsDeviceId odTvGsDeviceId, AppSettings appSettings)
        {
            _viewControl = viewControl;
            using var odTvGsDevice = odTvGsDeviceId.openObject(OdTv_OpenMode.kForRead);
            _odTvGsViewId = odTvGsDevice.getActiveView();
            _cadZoomOperations = new CadZoomOperations(_odTvGsViewId);
            _appSettings = appSettings;
        }

        public bool HandleMouseTouchDown<T>(T e, UserControl window) where T : InputEventArgs
        {
            var deviceId = e.GetDeviceId();

            TouchPoints.Add(deviceId);
            switch (TouchPoints.Count)
            {
                case 1:
                    if (TouchPoints.IndexOf(deviceId) == 0)
                    {
                        _firstOfTwoPoints = DpiScaledMousePosition(e, window);
                    }

                    break;
                case 2:
                    if (TouchPoints.IndexOf(deviceId) == 1)
                    {
                        _secondOfTwoPoints = DpiScaledMousePosition(e, window);
                    }
                    if (_firstOfTwoPoints == null || _secondOfTwoPoints == null)
                    {
                        return false;
                    }

                    //Calculate Initial distance between touch points
                    //_previousMaximumDist = HCLCoreMath.CalculateDistance(_firstOfTwoPoints.Value, _secondOfTwoPoints.Value);
                    //CADModel.GetCadBehavior(CadViewOperation.ZoomToScale).Init(CADModelConstants.ZoomScale);
                    _previousMaximumDist = ZoomHelper.CalculateDistance(_firstOfTwoPoints.Value, _secondOfTwoPoints.Value);
                    return true;
            }

            if (_appSettings.Interactivity)
            {
                //start Interactivity
                using var odTvGsView = _odTvGsViewId.openObject(OdTv_OpenMode.kForRead);
                odTvGsView.beginInteractivity(_appSettings.InteractiveFps);
            }
            return false;
        }
        public void HandleMouseTouchMove<T>(T e, UserControl window) where T : InputEventArgs
        {
            if (_firstOfTwoPoints == null || _secondOfTwoPoints == null)
            {
                return;
            }

            int touchDeviceId = e.GetDeviceId();
            var wpfLocation = e.GetPosition(window);
            var touchLocation = DpiScaledPoint(wpfLocation);

            switch (TouchPoints.IndexOf(touchDeviceId))
            {
                case 0:
                    _firstOfTwoPoints = touchLocation;

                    break;
                case 1:
                    _secondOfTwoPoints = touchLocation;

                    break;
            }

            double newDist = -1;

            if (TouchPoints.Count == 2)
            {
                //Calculate new distance on Move
                newDist = ZoomHelper.CalculateDistance(_firstOfTwoPoints.Value, _secondOfTwoPoints.Value);
            }

            if (!(Math.Abs(_previousMaximumDist - newDist) > CadModelConstants.ZoomThresholdDistance))
            {
                return;
            }

            //Calculate the AreaZooming scale from new and old distance
            var scale = newDist / _previousMaximumDist;
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

        public void HandleMouseTouchUp<T>(T e, UserControl window) where T : InputEventArgs
        {
            var touchDeviceId = e.GetDeviceId();

            if (TouchPoints.Contains(touchDeviceId))
            {
                if (touchDeviceId == TouchPoints[0])
                {
                    _firstOfTwoPoints = null;
                }
                else if (touchDeviceId == TouchPoints[1])
                {
                    _secondOfTwoPoints = null;
                }
                TouchPoints.Remove(touchDeviceId);
            }
            if (TouchPoints.Count == 0)
            {

            }
            _previousMaximumDist = -1;
            //end Interactivity
            if (_appSettings.Interactivity)
            {
                using var odTvGsView = _odTvGsViewId.openObject(OdTv_OpenMode.kForRead);
                odTvGsView.endInteractivity();
            }
        }

        public bool NoTouching() => TouchPoints.Count == 0;
    }
}

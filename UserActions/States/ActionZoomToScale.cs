// Copyright © 2018 by Hilti Corporation – all rights reserved

using System.Windows.Controls;

namespace HCL_ODA_TestPAD.UserActions.States
{
    public class ActionZoomToScale : UserActionStateBase
    {
        private readonly UserPinchZooming _userPinchZooming;
        public ActionZoomToScale(IUserActionManager manager) : base(manager)
        {
            CurrentAction = UserInteraction.ZoomToScale;
            _userPinchZooming = new UserPinchZooming(manager.CadImageViewControl, manager.VmAdapter.TvGsDeviceId, manager.AppSettings);
        }

        public override void ExecuteMouseTouchDown<T>(T e, UserControl window)
        {
            UserActionManager.IsPinchZooming = _userPinchZooming.HandleMouseTouchDown(e, window);
        }
        public override void ExecuteMouseTouchMove<T>(T e, UserControl window)
        {
            if (UserActionManager.IsPinchZooming)
            {
                _userPinchZooming.HandleMouseTouchMove(e, window);
            }
        }
        public override void ExecuteMouseTouchUp<T>(T e, UserControl window)
        {
            _userPinchZooming.HandleMouseTouchUp(e, window);
        }
        public override void ExecuteTouchLeave<T>(T e, UserControl window)
        {
            if (_userPinchZooming.NoTouching())
            {
                UserActionManager.IsPinchZooming = false;
            }
        }
    }
}
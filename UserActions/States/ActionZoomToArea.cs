// Copyright © 2018 by Hilti Corporation – all rights reserved

using System.Windows.Controls;

namespace HCL_ODA_TestPAD.UserActions.States
{
    public class ActionZoomToArea : UserActionStateBase
    {
        private readonly UserAreaZooming _userAreaZooming;
        public ActionZoomToArea(IUserActionManager manager) : base(manager)
        {
            CurrentAction = UserInteraction.ZoomToArea;
            _userAreaZooming = new UserAreaZooming(manager.CadImageViewControl, manager.VmAdapter.TvGsDeviceId, manager.AppSettings, manager.CanvasFactory, manager.BorderFactory);
        }

        public override void ExecuteMouseTouchDown<T>(T e, UserControl window)
        {
            _userAreaZooming.HandleMouseTouchDown(e, window);
        }
        public override void ExecuteMouseTouchMove<T>(T e, UserControl window)
        {
            if (CanMouseEvent())
            {
                _userAreaZooming.HandleMouseTouchMove(e, window);
            }
        }
        public override void ExecuteMouseTouchUp<T>(T e, UserControl window)
        {
            if (CanMouseEvent())
            {
                _userAreaZooming.HandleMouseTouchUp(e, window);
            }
        }
    }
}
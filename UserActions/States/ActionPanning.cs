// Copyright © 2018 by Hilti Corporation – all rights reserved

using System.Windows.Controls;
using System.Windows.Input;

namespace HCL_ODA_TestPAD.UserActions.States
{
    public class ActionPanning : UserActionStateBase
    {
        private readonly UserPanning _userPanning;

        public ActionPanning(IUserActionManager manager) : base(manager)
        {
            CurrentAction = UserInteraction.Panning;
            _userPanning = new UserPanning(manager.VmAdapter, manager.EventAggregator);
        }

        public override void ExecuteMouseTouchDown<T>(T e, UserControl window)
        {
            _userPanning.HandleMouseTouchDown(e as MouseButtonEventArgs, window);
        }
        public override void ExecuteMouseTouchMove<T>(T e, UserControl window)
        {
            if (CanMouseEvent())
            {
                _userPanning.HandleMouseTouchMove(e as MouseEventArgs, window);
            }
        }
        public override void ExecuteMouseTouchUp<T>(T e, UserControl window)
        {
            if (CanMouseEvent())
            {
                _userPanning.HandleMouseTouchUp(e as MouseButtonEventArgs, window);
            }
        }
    }
}
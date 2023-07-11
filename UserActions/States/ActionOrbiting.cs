// Copyright © 2018 by Hilti Corporation – all rights reserved

using System.Windows.Controls;
using System.Windows.Input;

namespace HCL_ODA_TestPAD.UserActions.States
{
    public class ActionOrbiting : UserActionStateBase
    {
        private readonly UserOrbiting _userOrbiting;

        public ActionOrbiting(IUserActionManager manager) : base(manager)
        {
            CurrentAction = UserInteraction.Orbiting;
            _userOrbiting = new UserOrbiting(manager.VmAdapter, manager.EventAggregator);
        }

        public override void ExecuteMouseTouchDown<T>(T e, UserControl window)
        {
            _userOrbiting.HandleMouseTouchDown(e as MouseButtonEventArgs, window);
        }
        public override void ExecuteMouseTouchMove<T>(T e, UserControl window)
        {
            if (CanMouseEvent())
            {
                _userOrbiting.HandleMouseTouchMove(e as MouseEventArgs, window);
            }
        }
        public override void ExecuteMouseTouchUp<T>(T e, UserControl window)
        {
            _userOrbiting.HandleMouseTouchUp(e as MouseButtonEventArgs, window);
        }
    }
}
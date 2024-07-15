// Copyright © 2018 by Hilti Corporation – all rights reserved

using System.Windows.Controls;

namespace HCL_ODA_TestPAD.HCL.UserActions.States
{
    public class ActionIdle : UserActionStateBase
    {
        public ActionIdle(IUserActionManager manager) : base(manager)
        {
            CurrentAction = UserInteraction.Idle;
        }

        public override void ExecuteMouseTouchDown<T>(T e, UserControl window)
        {
            UserActionManager.DoIdle(CurrentAction);
        }
        public override void ExecuteMouseTouchMove<T>(T e, UserControl window)
        {
            UserActionManager.DoIdle(CurrentAction);
        }
        public override void ExecuteMouseTouchUp<T>(T e, UserControl window)
        {
            UserActionManager.DoIdle(CurrentAction);
        }
    }
}
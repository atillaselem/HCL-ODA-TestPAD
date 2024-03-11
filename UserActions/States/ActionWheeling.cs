// Copyright © 2018 by Hilti Corporation – all rights reserved

using System;
using System.Windows.Input;

namespace HCL_ODA_TestPAD.UserActions.States
{
    public class ActionWheeling : UserActionStateBase
    {
        public ActionWheeling(IUserActionManager manager) : base(manager)
        {
            CurrentAction = UserInteraction.Wheeling;
        }
        public override IUserActionState DoStateTransition(UserInteraction userAction)
            => userAction == CurrentAction ? this : userAction switch
            {
                UserInteraction.Panning or UserInteraction.Orbiting or UserInteraction.ZoomToArea => UserActionManager.GetActionState(userAction),
                _ => throw new ArgumentOutOfRangeException(nameof(userAction), userAction, null)
            };

        public override IUserActionState ExecuteWheeling(MouseWheelEventArgs e, UserInteraction selectedAction, Action zoomNotPossible)
        {
            DoWheeling(e, zoomNotPossible);
            return DoStateTransition(selectedAction);
        }
        private void DoWheeling(MouseWheelEventArgs e, Action zoomNotPossible)
        {
            ArgumentNullException.ThrowIfNull(e);
            ArgumentNullException.ThrowIfNull(zoomNotPossible);
            UserActionManager.VmAdapter.MouseWheel(e);
            UserActionManager.IsWheeling = true;
        }
    }
}
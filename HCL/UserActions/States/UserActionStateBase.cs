// Copyright © 2018 by Hilti Corporation – all rights reserved

using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace HCL_ODA_TestPAD.HCL.UserActions.States;

public abstract class UserActionStateBase : IUserActionState
{
    protected UserInteraction CurrentAction { get; set; }
    protected UserInteraction PreviousInteraction { get; set; }
    protected IUserActionManager UserActionManager { get; set; }

    protected UserActionStateBase(IUserActionManager manager)
    {
        ArgumentNullException.ThrowIfNull(manager);
        UserActionManager = manager;
    }

    /// <summary>
    /// Override this method for any derived state to prevent any state transition from one to another according the business rule.
    /// By default any state can be transited from another by user interaction.
    /// ActionWheeling overrides this method to limit state transitions.
    /// </summary>
    /// <param name="userAction"></param>
    /// <returns>Actual or new state depending on the userAction</returns>
    public virtual IUserActionState DoStateTransition(UserInteraction userAction)
        => userAction == CurrentAction ? this :
        UserActionManager.GetActionState(userAction);

    public virtual void ExecuteMouseTouchDown<T>(T e, UserControl window) where T : InputEventArgs { }
    public virtual void ExecuteMouseTouchMove<T>(T e, UserControl window) where T : InputEventArgs { }
    public virtual void ExecuteMouseTouchUp<T>(T e, UserControl window) where T : InputEventArgs { }
    public virtual void ExecuteTouchLeave<T>(T e, UserControl window) where T : InputEventArgs { }

    /// <summary>
    /// This method is used to improve Touching and Wheeling performance by skipping multiple CAD Update during Pinch Zooming and Mouse Wheeling.
    /// MouseMove events will be skipped.
    /// </summary>
    public bool CanMouseEvent()
    {
        if (UserActionManager.IsWheeling)
        {
            return UserActionManager.IsWheeling = false;
        }

        return !UserActionManager.IsPinchZooming;
    }

    public virtual IUserActionState ExecuteWheeling(MouseWheelEventArgs e, UserInteraction selectedAction, Action zoomNotPossible)
    {
        throw new NotImplementedException();
    }

    public UserInteraction ActiveAction => CurrentAction;

    public override string ToString()
    {
        return $"CurrentState - {CurrentAction}";
    }
}
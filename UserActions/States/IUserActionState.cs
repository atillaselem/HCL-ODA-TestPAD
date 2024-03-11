// Copyright © 2018 by Hilti Corporation – all rights reserved

using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace HCL_ODA_TestPAD.UserActions.States
{
    public enum UserInteraction
    {
        Idle,
        Panning,
        Orbiting,
        ZoomToArea,
        ZoomToScale,
        Wheeling
    }
    public interface IUserActionState
    {
        IUserActionState DoStateTransition(UserInteraction userAction);
        void ExecuteMouseTouchDown<T>(T e, UserControl window) where T : InputEventArgs;
        void ExecuteMouseTouchMove<T>(T e, UserControl window) where T : InputEventArgs;
        void ExecuteMouseTouchUp<T>(T e, UserControl window) where T : InputEventArgs;
        void ExecuteTouchLeave<T>(T e, UserControl window) where T : InputEventArgs;
        IUserActionState ExecuteWheeling(MouseWheelEventArgs e, UserInteraction selectedAction, Action zoomNotPossible);
        UserInteraction ActiveAction { get; }
    }
}
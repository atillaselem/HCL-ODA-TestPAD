// Copyright © 2018 by Hilti Corporation – all rights reserved

using System.Windows;
using System.Windows.Input;

namespace HCL_ODA_TestPAD.HCL.UserActions;

public static class InputEventArgsExtensions
{
    public static Point GetPosition<T>(this T e, IInputElement obj)
        where T : InputEventArgs
    => e switch
    {
        MouseEventArgs args => args.GetPosition(obj),
        TouchEventArgs args => args.GetTouchPoint(obj).Position,
        _ => new Point()
    };
    public static int GetDeviceId<T>(this T e)
        where T : InputEventArgs
    => e switch
    {
        MouseEventArgs args => args.Device.GetHashCode(),
        TouchEventArgs args => args.TouchDevice.Id,
        _ => new int()
    };
}
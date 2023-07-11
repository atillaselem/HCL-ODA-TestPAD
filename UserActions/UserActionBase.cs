// Copyright © 2018 by Hilti Corporation – all rights reserved

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using HCL_ODA_TestPAD.HCL;

namespace HCL_ODA_TestPAD.UserActions;

public class UserActionBase
{
    protected IList<int> TouchPoints { get; } = new List<int>();

    protected virtual Point DpiScaledMousePosition<T>(T e, UserControl window) where T : InputEventArgs
    {
        var wpfLocation = e.GetPosition(window);

        return DpiScaledPoint(wpfLocation);
    }

    protected static Point DpiScaledPoint(Point wpfLocation)
    {
        return CadScreenInfoProvider.DpiScaledPoint(wpfLocation);
    }

    protected virtual void ResetTouchFlags()
    {
        TouchPoints.Clear();
    }
}
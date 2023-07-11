// Copyright © 2018 by Hilti Corporation – all rights reserved

using System;
using System.Windows.Controls;
using System.Windows.Input;
using HCL_ODA_TestPAD.HCL;
using HCL_ODA_TestPAD.ViewModels;
using Prism.Events;

namespace HCL_ODA_TestPAD.UserActions
{
    public class UserOrbiting : UserActionBase
    {
        private readonly HclCadImageViewModel _vmAdapter;
        private readonly IEventAggregator _eventAggregator;

        public UserOrbiting(HclCadImageViewModel vmAdapter, IEventAggregator eventAggregator)
        {
            _vmAdapter = vmAdapter;
            _eventAggregator = eventAggregator;
        }

        public void HandleMouseTouchDown(MouseButtonEventArgs e, UserControl window)
        {
            ArgumentNullException.ThrowIfNull(e);
            _vmAdapter.MouseDown(e, e.GetPosition(window));
        }
        public void HandleMouseTouchMove(MouseEventArgs e, UserControl window)
        {
            ArgumentNullException.ThrowIfNull(e);
            _vmAdapter.MouseMove(e, e.GetPosition(window));
        }

        public void HandleMouseTouchUp(MouseButtonEventArgs e, UserControl window)
        {
            ArgumentNullException.ThrowIfNull(e);
            _vmAdapter.MouseUp(e);
        }
    }

}

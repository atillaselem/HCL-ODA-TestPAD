// Copyright © 2018 by Hilti Corporation – all rights reserved

using System.Windows.Input;
using HCL_ODA_TestPAD.ViewModels;
using Prism.Events;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using UserControl = System.Windows.Controls.UserControl;

namespace HCL_ODA_TestPAD.UserActions
{
    public class UserPanning : UserActionBase
    {
        private readonly HclCadImageViewModel _vmAdapter;
        private readonly IEventAggregator _eventAggregator;

        public UserPanning(HclCadImageViewModel vmAdapter, IEventAggregator eventAggregator)
        {
            _vmAdapter = vmAdapter;
            _eventAggregator = eventAggregator;
        }

        public void HandleMouseTouchDown(MouseButtonEventArgs e, UserControl window)
        {
            _vmAdapter.MouseDown(e, e.GetPosition(window));
        }
        public void HandleMouseTouchMove(MouseEventArgs e, UserControl window)
        {
            _vmAdapter.MouseMove(e, e.GetPosition(window));
        }

        public void HandleMouseTouchUp(MouseButtonEventArgs e, UserControl window)
        {
            _vmAdapter.MouseUp(e);
        }
    }

}

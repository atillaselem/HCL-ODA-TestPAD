// Copyright © 2018 by Hilti Corporation – all rights reserved

using System;
using System.Collections.Generic;
using System.Windows.Controls;
using HCL_ODA_TestPAD.Settings;
using HCL_ODA_TestPAD.ViewModels;
using HCL_ODA_TestPAD.ViewModels.Base;
using Prism.Events;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace HCL_ODA_TestPAD.UserActions.States
{
    public interface IUserActionManager
    {
        ICadImageViewControl CadImageViewControl { get; }
        AppSettings AppSettings { get; }
        IEventAggregator EventAggregator { get; }
        Func<Canvas> CanvasFactory { get; }
        Func<Border> BorderFactory { get; }
        bool IsWheeling { get; set; }
        bool IsPinchZooming { get; set; }
        IUserActionState GetActionState(UserInteraction userAction);
        HclCadImageViewModel VmAdapter { get; }
    }

    public class UserActionManager : IUserActionManager
    {
        private readonly Dictionary<UserInteraction, IUserActionState> _stateDict = new();

        public UserActionManager(IEventAggregator eventAggregator, Func<Canvas> canvasFactory,
                                 Func<Border> borderFactory, 
                                 ICadImageViewControl cadImageViewControl,
                                 AppSettings appSettings,
                                 HclCadImageViewModel vmAdapter)
        {
            EventAggregator = eventAggregator;
            CanvasFactory = canvasFactory;
            BorderFactory = borderFactory;
            CadImageViewControl = cadImageViewControl;
            AppSettings = appSettings;
            VmAdapter = vmAdapter;
        }

        public ICadImageViewControl CadImageViewControl { get; }
        public AppSettings AppSettings { get; }

        public IEventAggregator EventAggregator { get; }

        public Func<Canvas> CanvasFactory { get; }

        public Func<Border> BorderFactory { get; }

        public ILogger Logger { get; set; }

        public bool IsWheeling { get; set; }
        public bool IsPinchZooming { get; set; }

        public IUserActionState GetActionState(UserInteraction userAction)
        {
            if (!_stateDict.ContainsKey(userAction))
            {
                _stateDict.Add(userAction, this.CreateState(userAction));
            }

            return _stateDict[userAction];
        }

        public HclCadImageViewModel VmAdapter { get; }
    }
}

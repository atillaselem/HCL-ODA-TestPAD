using System;
using HCL_ODA_TestPAD.Mvvm;
using HCL_ODA_TestPAD.Services;
using HCL_ODA_TestPAD.Settings;
using Prism.Events;

namespace HCL_ODA_TestPAD.ViewModels
{
    public class TestPADSettingsViewModel : BindableBase
    {
        public ISettingsProvider SettingsProvider { get; }
        private readonly IEventAggregator _eventAggregator;
        private readonly IConsoleService _consoleService;
        private IAppSettings _appSettings;
        public Func<IEventAggregator> EventFactory { get; private set; }
        public TestPADSettingsViewModel(IEventAggregator eventAggregator,
            IConsoleService consoleService, 
            ISettingsProvider settingsProvider)
        {
            SettingsProvider = settingsProvider;
            _eventAggregator = eventAggregator;
            _consoleService = consoleService;
            _appSettings = settingsProvider.AppSettings;
            EventFactory = () => _eventAggregator;
        }
        public IAppSettings AppSettings
        {
            get => _appSettings;
            set => SetProperty(ref _appSettings, value);
        }
    }
}

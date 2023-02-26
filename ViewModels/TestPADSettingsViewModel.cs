using HCL_ODA_TestPAD.Mvvm;
using HCL_ODA_TestPAD.Services;
using HCL_ODA_TestPAD.Settings;
using Prism.Events;

namespace HCL_ODA_TestPAD.ViewModels
{
    public class TestPADSettingsViewModel : BindableBase
    {
        public ISettingsProvider TrSettings { get; }
        private readonly IEventAggregator _eventAggregator;
        private readonly IConsoleService _consoleService;
        private IAppSettings _appSettings;
        public TestPADSettingsViewModel(IEventAggregator eventAggregator,
            IConsoleService consoleService, 
            ISettingsProvider trSettings)
        {
            TrSettings = trSettings;
            _eventAggregator = eventAggregator;
            _consoleService = consoleService;
            _appSettings = trSettings.AppSettings;
        }
        public IAppSettings AppSettings
        {
            get => _appSettings;
            set => SetProperty(ref _appSettings, value);
        }
    }
}

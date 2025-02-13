using System;
using HCL_ODA_TestPAD.Mvvm;
using HCL_ODA_TestPAD.Settings;
using Prism.Events;

namespace HCL_ODA_TestPAD.ViewModels
{
    public class TestPadSettingsViewModel : BindableBase
    {
        public TestPadSettings TestPadSettings { get; set; }
        private readonly IServiceFactory _serviceFactory;
        private AppSettings _appSettings;
        public Func<IEventAggregator> EventFactory { get; private set; }
        public TestPadSettingsViewModel(IServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
            EventFactory = () => _serviceFactory.EventSrv;
            _appSettings = _serviceFactory.AppSettings;
        }
        public AppSettings AppSettings
        {
            get => _appSettings;
            set => SetProperty(ref _appSettings, value);
        }

    }
}

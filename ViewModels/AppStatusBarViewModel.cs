using HCL_ODA_TestPAD.Mvvm;
using HCL_ODA_TestPAD.Mvvm.Events;
using Prism.Events;

namespace HCL_ODA_TestPAD.ViewModels
{
    public class AppStatusBarViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        //private readonly ILogger<AppStatusBarViewModel> _logger;


        public AppStatusBarViewModel(IEventAggregator eventAggregator/*, ILogger<AppStatusBarViewModel> logger*/)
        {
            _eventAggregator = eventAggregator;
            //_logger = logger;
            _eventAggregator.GetEvent<FibexFileLoadedEvent>().Subscribe(OnFibexFileLoaded);
        }

        private void OnFibexFileLoaded(FibexFileLoadedEventArgs obj)
        {
            AppStatus = "HCL-ODA-TestPAD loaded successfully";
            //_logger.LogCritical(AppStatus);
            //_logger.LogError(AppStatus);
            //_logger.LogWarning(AppStatus);
            //_logger.LogInformation(AppStatus);
            //_logger.LogDebug(AppStatus);
            //_logger.LogTrace(AppStatus);
        }

        private string _appStatus;
        public string AppStatus
        {
            get => _appStatus;
            set => SetProperty(ref _appStatus, value);
        }
    }
}

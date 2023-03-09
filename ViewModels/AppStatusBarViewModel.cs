using HCL_ODA_TestPAD.Mvvm;
using HCL_ODA_TestPAD.Mvvm.Events;
using Prism.Events;
using System.Windows.Media;
using System.Windows;
using System;
using HCL_ODA_TestPAD.Settings;

namespace HCL_ODA_TestPAD.ViewModels
{
    public class AppStatusBarViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IAppSettings _appSettings;
        private string _statusMessage;
        private int _progressValue;
        private int _progressMax;
        private double _progressPercentage;
        private Visibility _progressBarVisibility = Visibility.Collapsed;

        private LinearGradientBrush _exceptionBrush = new LinearGradientBrush(Colors.LightPink, Colors.IndianRed, new Point(0.5, 0), new Point(0.5, 1));
        private LinearGradientBrush _brushStatusBar = new LinearGradientBrush(Colors.LightGray, Colors.DarkGray, new Point(0.5, 0), new Point(0.5, 1));
        public AppStatusBarViewModel(IEventAggregator eventAggregator,
        ISettingsProvider settingsProvider)
        {
            _eventAggregator = eventAggregator;
            _appSettings = settingsProvider.AppSettings;
            StatusMessage = "HCL-ODA-TestPAD loaded successfully";
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            _eventAggregator.GetEvent<ProgressStepChangedEvent>().Subscribe(OnProgressChangedEvent);
            _eventAggregator.GetEvent<ProgressMaxChangedEvent>().Subscribe(OnProgressMaxEvent);
            _eventAggregator.GetEvent<AppStatusTextChanged>().Subscribe(OnAppStatusTextChanged);
        }

        private void OnAppStatusTextChanged(string statusText)
        {
            StatusMessage = statusText;
        }

        private void OnProgressMaxEvent(int progressMax)
        {
            ProgressMax = progressMax;
            ProgressPercentage = (100.0 * ProgressValue) / ProgressMax;
        }

        private void OnProgressChangedEvent(int progressCurrent)
        {
            if (Math.Abs(ProgressPercentage - 100.0) > 0.001)
            {
                ProgressValue = progressCurrent;
                ProgressPercentage = (100.0 * ProgressValue) / ProgressMax;
            }
        }

        public LinearGradientBrush MessageStatusColor
        {
            get => _brushStatusBar;
            set => SetProperty(ref _brushStatusBar, value);
        }
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }
        public int ProgressValue
        {
            get => _progressValue;
            set
            {
                SetProperty(ref _progressValue, value);
                if (_progressValue > 0)
                {
                    ProgressBarVisibility = Visibility.Visible;
                }
            }
        }
        public int ProgressMax
        {
            get => _progressMax;
            set => SetProperty(ref _progressMax, value);
        }
        public double ProgressPercentage
        {
            get => _progressPercentage;
            set => SetProperty(ref _progressPercentage, value);
        }
        public Visibility ProgressBarVisibility
        {
            get => _progressBarVisibility;
            set => SetProperty(ref _progressBarVisibility, value);
        }
    }
}

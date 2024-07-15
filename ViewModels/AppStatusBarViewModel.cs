using HCL_ODA_TestPAD.Mvvm;
using HCL_ODA_TestPAD.Mvvm.Events;
using System.Windows.Media;
using System.Windows;
using System;
using HCL_ODA_TestPAD.Settings;

namespace HCL_ODA_TestPAD.ViewModels
{
    public class AppStatusBarViewModel : BindableBase
    {
        private readonly IServiceFactory _serviceFactory;
        private string _statusMessage;
        private string _screenCoordinates;
        private string _cadCoordinates;
        private string _scaleFactor;
        private int _progressValue;
        private int _progressMax;
        private string _progressInfo;
        private Visibility _progressBarVisibility = Visibility.Collapsed;

        private LinearGradientBrush _exceptionBrush = new LinearGradientBrush(Colors.LightPink, Colors.IndianRed, new Point(0.5, 0), new Point(0.5, 1));
        private LinearGradientBrush _brushStatusBar = new LinearGradientBrush(Colors.LightGray, Colors.DarkGray, new Point(0.5, 0), new Point(0.5, 1));
        public AppStatusBarViewModel(IServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
            StatusMessage = "HCL-ODA-TestPAD loaded successfully";
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            _serviceFactory.EventSrv.GetEvent<ProgressStepChangedEvent>().Subscribe(OnProgressStepChangedEvent);
            _serviceFactory.EventSrv.GetEvent<ProgressMaxChangedEvent>().Subscribe(OnProgressMaxEvent);
            _serviceFactory.EventSrv.GetEvent<AppStatusTextChanged>().Subscribe(OnAppStatusTextChanged);
            _serviceFactory.EventSrv.GetEvent<ScreenCoordinatesChanged>().Subscribe(OnScreenCoordinatesChanged);
            _serviceFactory.EventSrv.GetEvent<ScaleFactorChanged>().Subscribe(OnScaleFactorChanged);
        }

        private void OnAppStatusTextChanged(string statusText)
        {
            StatusMessage = statusText;
        }
        private void OnScreenCoordinatesChanged(string coordinates)
        {
            ScreenCoordinates = coordinates.Split(';')[0];
            CadCoordinates = coordinates.Split(';')[1];
        }
        private void OnScaleFactorChanged(string scaleFactor)
        {
            ScaleFactor = scaleFactor;
        }

        private void OnProgressMaxEvent(int progressMax)
        {
            ProgressMax = progressMax;
            ProgressValue = 0;
            ProgressInfo = String.Empty;
        }

        private void OnProgressStepChangedEvent(ProgressStepChangedEventArg eventArg)
        {
            if (Math.Abs(ProgressValue - 100.0) > 0.001)
            {
                ProgressValue += eventArg.CurrentProgressStep;
            }
            ProgressInfo = $"{eventArg.CurrentDeviceCoefficient}/{eventArg.RegenThreshold}/{eventArg.LastDeviceCoefficientAfterRegen}";
            if(ProgressValue > 100)
            {
                ProgressValue = 0;
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
                else
                {
                    ProgressBarVisibility = Visibility.Hidden;
                }
            }
        }
        public int ProgressMax
        {
            get => _progressMax;
            set => SetProperty(ref _progressMax, value);
        }
        public string ProgressInfo
        {
            get => _progressInfo;
            set => SetProperty(ref _progressInfo, value);
        }
        public Visibility ProgressBarVisibility
        {
            get => _progressBarVisibility;
            set => SetProperty(ref _progressBarVisibility, value);
        }
        public string ScreenCoordinates
        {
            get => _screenCoordinates;
            set => SetProperty(ref _screenCoordinates, value);
        }
        public string CadCoordinates
        {
            get => _cadCoordinates;
            set => SetProperty(ref _cadCoordinates, value);
        }
        public string ScaleFactor
        {
            get => _scaleFactor;
            set => SetProperty(ref _scaleFactor, value);
        }
    }
}

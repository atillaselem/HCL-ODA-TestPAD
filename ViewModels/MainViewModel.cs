using HCL_ODA_TestPAD.Mvvm;
using HCL_ODA_TestPAD.Mvvm.Events;
using HCL_ODA_TestPAD.ODA;
using HCL_ODA_TestPAD.Settings;
using HCL_ODA_TestPAD.Splash;
using Prism.Events;
using System;
using System.Windows;
using Teigha.Visualize;

namespace HCL_ODA_TestPAD.ViewModels;

public class MainViewModel : BindableBase
{
    private readonly IEventAggregator _eventAggregator;
    public AppAvalonDockViewModel AppAvalonDockViewModel { get; }
    public AppMenuViewModel AppMenuViewModel { get; }
    public AppStatusBarViewModel AppStatusBarViewModel { get; }
    private readonly ISettingsProvider _settingsProvider;

    public MainViewModel(AppMenuViewModel appMenuViewModel,
        AppAvalonDockViewModel appAvalonDockViewModel,
        AppStatusBarViewModel appStatusBarViewModel, 
        IEventAggregator eventAggregator,
        ISettingsProvider settingsProvider)
    {
        AppAvalonDockViewModel = appAvalonDockViewModel;
        AppMenuViewModel = appMenuViewModel;
        AppStatusBarViewModel = appStatusBarViewModel;
        _eventAggregator = eventAggregator;
        _settingsProvider = settingsProvider;
        SubscribeEvents();
    }

    private void SubscribeEvents()
    {
        _eventAggregator.GetEvent<ExitApplicationEvent>().Subscribe(OnExitApplication);
    }

    private void OnExitApplication()
    {
        //Triggers Windows.OnClosing Event
        Application.Current.Shutdown();
    }

    public void ActivateODA()
    {
        try
        {
            TeighaActivate();
            TV_Globals.odTvInitialize();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Product Activation Error!\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
        }
    }

    private void TeighaActivate()
    {
        Teigha.Core.Globals.odActivate(ActivationData.userInfo, ActivationData.userSignature);
    }

    public void TeighaDeactivate()
    {
        try
        {
            TV_Globals.odTvUninitialize();
            Teigha.Core.Globals.odCleanUpStaticData();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Ñan not deactivate the product!\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
        }
    }

    public void DoAppSpecificClosing()
    {
        _eventAggregator.GetEvent<UnLoadTabViewEvent>().Publish();
        if (_settingsProvider.AppSettings.SaveSettings)
        {
            _settingsProvider.SaveSettings();
        }
    }

    public void ShowSplashScreen()
    {
        if (_settingsProvider.AppSettings.ShowSplashScreen)
        {
            SplashTestPAD.ShowSplashScreenEvents();
        }
    }
}

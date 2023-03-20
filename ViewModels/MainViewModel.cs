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
    private readonly IServiceFactory _serviceFactory;
    public AppAvalonDockViewModel AppAvalonDockViewModel { get; }
    public AppMenuViewModel AppMenuViewModel { get; }
    public AppStatusBarViewModel AppStatusBarViewModel { get; }

    public MainViewModel(AppMenuViewModel appMenuViewModel,
        AppAvalonDockViewModel appAvalonDockViewModel,
        AppStatusBarViewModel appStatusBarViewModel,
        IServiceFactory serviceFactory)
    {
        _serviceFactory = serviceFactory;
        AppAvalonDockViewModel = appAvalonDockViewModel;
        AppMenuViewModel = appMenuViewModel;
        AppStatusBarViewModel = appStatusBarViewModel;
        SubscribeEvents();
    }

    private void SubscribeEvents()
    {
        _serviceFactory.EventSrv.GetEvent<ExitApplicationEvent>().Subscribe(OnExitApplication);
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
        _serviceFactory.EventSrv.GetEvent<UnLoadTabViewEvent>().Publish();
        if (_serviceFactory.AppSettings.SaveSettings)
        {
            _serviceFactory.SettingsSrv.SaveSettings(_serviceFactory.AppSettings);
        }
    }

    public void ShowSplashScreen()
    {
        if (_serviceFactory.AppSettings.ShowSplashScreen)
        {
            SplashTestPAD.ShowSplashScreenEvents();
        }
    }
}

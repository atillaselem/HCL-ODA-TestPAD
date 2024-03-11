using HCL_ODA_TestPAD.ODA;
using HCL_ODA_TestPAD.Settings;
using HCL_ODA_TestPAD.Splash;
using HCL_ODA_TestPAD.Utility;
using HCL_ODA_TestPAD.ViewModels;
using ODA.Kernel.TD_RootIntegrated;
using ODA.Visualize.TV_Visualize;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace HCL_ODA_TestPAD;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public static MainWindowViewModel ViewModel = null;
    private readonly ISettingsProvider _settingsProvider;

    public static bool IsClosing { get; private set; }

    public MainWindow(MainWindowViewModel viewModel,
        ISettingsProvider settingsProvider)
    {
        InitializeComponent();
        Title = AssemblyHelper.GetAppTitle();

        _settingsProvider = settingsProvider;

        try
        {
            TeighaActivate();
            TV_Visualize_Globals.odTvInitialize();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Product Activation Error!\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
        }

        ViewModel = viewModel;
        DataContext = ViewModel;

        ViewModel.AppMainWindow = this;

        ViewModel.FileIsExist = false;
        this.Closing += OdTvWpfMainWindow_Closing;

        if (_settingsProvider.AppSettings.RenderDevice == RenderDevice.OpenGL_Bitmap)
        {
            ViewModel.AddView(true);
        }
    }
    private void WindowInitializing(object sender, EventArgs e)
    {
        if (_settingsProvider.AppSettings.ShowSplashScreen)
        {
            SplashTestPAD.ShowSplashScreenEvents();
        }
    }

    private void TeighaActivate()
    {
        TD_RootIntegrated_Globals.odActivate(ActivationData.userInfo, ActivationData.userSignature);
    }

    private void TeighaDeactivate()
    {
        TD_RootIntegrated_Globals.odCleanUpStaticData();
    }

    // Clear all devices before close, need for correct finish odTvUninitialize()
    private void OdTvWpfMainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if(IsClosing) {
            if (ViewModel != null)
                ViewModel.ClearRenderArea();
            IsClosing = false;
            e.Cancel = !IsClosing;
            return;
        }
        ViewModel.SaveSettings();
        ViewModel.ClearRenderArea();
        UnInitializeOdVisualize();
        e.Cancel = false;
    }

    private void UnInitializeOdVisualize()
    {
        try
        {
            TV_Visualize_Globals.odTvUninitialize();
            TeighaDeactivate();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Ñan not deactivate the product!\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
        }
    }
    // Exit click event
    private void Close_Click(object sender, RoutedEventArgs e)
    {
        IsClosing = true;
        Close();
    }
    // Exit click event
    private void Exit_Click(object sender, RoutedEventArgs e)
    {       
        Close();
    }

    private void PropertiesSplitter_OnMouseMove(object sender, MouseEventArgs e)
    {
        GridSplitter gs = sender as GridSplitter;
        if (gs == null || !gs.IsDragging)
            return;
        //ViewModel.CurrentPropertiesWidth = PropertiesColumn.ActualWidth;
    }

    private void PropertiesSplitter_OnDragCompleted(object sender, DragCompletedEventArgs e)
    {
        //ViewModel.CurrentPropertiesWidth = PropertiesColumn.ActualWidth;
    }
}

using HCL_ODA_TestPAD.ODA;
using HCL_ODA_TestPAD.Settings;
using HCL_ODA_TestPAD.Splash;
using HCL_ODA_TestPAD.Utility;
using HCL_ODA_TestPAD.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Teigha.Visualize;

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
            TV_Globals.odTvInitialize();
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

        ContextMenu menu = new ContextMenu()
        {
            Background = new SolidColorBrush(Colors.White)
        };

        menu.Items.Add(new Separator());
        MenuItem openItm = new MenuItem() { Header = "Open", Height = 30 };
        openItm.Icon = new Image
        {
            Source = Application.Current.Resources["OpenImg"] as BitmapImage
        };
        menu.Items.Add(openItm);
        openItm.Command = ViewModel.OpenCommand;


        MenuItem saveItm = new MenuItem() { Header = "Save", Height = 30 };
        saveItm.Icon = new Image
        {
            Source = Application.Current.Resources["SaveImg"] as BitmapImage
        };
        //menu.Items.Add(saveItm);
        saveItm.Command = ViewModel.SaveCommand;
        MenuItem saveAsItm = new MenuItem() { Header = "SaveAs", Height = 30 };
        saveAsItm.Icon = new Image
        {
            Source = Application.Current.Resources["SaveAsImg"] as BitmapImage
        };
        //menu.Items.Add(saveAsItm);
        saveAsItm.Command = ViewModel.SaveAsCommand;
        //menu.Items.Add(new Separator());
        MenuItem pdf2dItm = new MenuItem() { Header = "Export to 2D PDF", Height = 30 };
        pdf2dItm.Icon = new Image
        {
            Source = Application.Current.Resources["PdfImg"] as BitmapImage
        };
        pdf2dItm.Command = ViewModel.Export2dPdfCommand;
        //menu.Items.Add(pdf2dItm);
        MenuItem pdf3dItem = new MenuItem() { Header = "Export to 3D PDF", Height = 30 };
        pdf3dItem.Icon = new Image
        {
            Source = Application.Current.Resources["PublishImg"] as BitmapImage
        };
        pdf3dItem.Command = ViewModel.Export3dPdfCommand;
        //menu.Items.Add(pdf3dItem);
        MenuItem closeItm = new MenuItem() { Header = "Close", Height = 30 };
        menu.Items.Add(closeItm);
        closeItm.Click += Close_Click;

        menu.Items.Add(new Separator());
        MenuItem settingsItem = new MenuItem() { Header = "Settings", Height = 30 };
        menu.Items.Add(settingsItem);
        settingsItem.Command = ViewModel.SettingsCommand;

        menu.Items.Add(new Separator());
        MenuItem exitItm = new MenuItem() { Header = "Exit", Height = 30 };
        menu.Items.Add(exitItm);
        exitItm.Click += Exit_Click;

        DropMenuBtn.DropDown = menu;

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
        Teigha.Core.Globals.odActivate(ActivationData.userInfo, ActivationData.userSignature);
    }

    private void TeighaDeactivate()
    {
        Teigha.Core.Globals.odCleanUpStaticData();
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
            TV_Globals.odTvUninitialize();
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

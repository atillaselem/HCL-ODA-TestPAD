using HCL_ODA_TestPAD.Services;
using HCL_ODA_TestPAD.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.AvalonDock.Themes;

namespace HCL_ODA_TestPAD.Views
{

    /// <summary>
    /// Interaction logic for AvalonDockPanel.xaml
    /// </summary>
    public partial class AppAvalonDockView : UserControl
    {
        public IConsoleService ConsoleService { get; set; }


        public AppAvalonDockView()
        {
            InitializeComponent();
            Dispatcher.ShutdownStarted += MainWindowClosing;
            _dockingManager.Theme = new VS2010Theme();
            Loaded += AppAvalonDockView_Loaded;
        }

        private void AppAvalonDockView_Loaded(object sender, RoutedEventArgs e)
        {
            var VM = DataContext as AppAvalonDockViewModel;
            VM?.LoadDockLayout(() => _dockingManager);
        }

        private void MainWindowClosing(object sender, EventArgs e)
        {
            var VM = DataContext as AppAvalonDockViewModel;
            VM?.SaveLayout(() => _dockingManager);
        }
    }
}

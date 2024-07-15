using HCL_ODA_TestPAD.Utility;
using HCL_ODA_TestPAD.ViewModels;
using System;
using System.ComponentModel;
using System.Windows;

namespace HCL_ODA_TestPAD.Views;

/// <summary>
/// Interaction logic for MainView.xaml
/// </summary>
public partial class MainView : Window
{
    private MainViewModel _vm;

    public MainView(MainViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
        _vm = vm;
        _vm.ActivateOda();
        Closing += OnClosing;
        Title = AssemblyHelper.GetAppTitle();
    }

    private void WindowInitializing(object sender, EventArgs e)
    {
        _vm.ShowSplashScreen();
    }

    private void OnClosing(object sender, CancelEventArgs e)
    {
        _vm.DoAppSpecificClosing();
        _vm.TeighaDeactivate();
    }

}

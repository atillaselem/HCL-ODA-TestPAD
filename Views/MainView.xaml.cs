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
    private MainViewModel VM;

    public MainView(MainViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
        VM = vm;
        VM.ActivateODA();
        Closing += OnClosing;
        Title = AssemblyHelper.GetAppTitle();
    }

    private void WindowInitializing(object sender, EventArgs e)
    {
        VM.ShowSplashScreen();
    }

    private void OnClosing(object sender, CancelEventArgs e)
    {
        VM.DoAppSpecificClosing();
        VM.TeighaDeactivate();
    }

}

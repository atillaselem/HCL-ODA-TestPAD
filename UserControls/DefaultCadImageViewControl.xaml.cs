using HCL_ODA_TestPAD.Dialogs;
using HCL_ODA_TestPAD.ODA.Draggers.Construct;
using HCL_ODA_TestPAD.ODA.Draggers.Navigation;
using HCL_ODA_TestPAD.Performance;
using HCL_ODA_TestPAD.ViewModels;
using HCL_ODA_TestPAD.ODA.WCS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Teigha.Core;
using Teigha.Visualize;
using static HCL_ODA_TestPAD.ViewModels.MainWindowViewModel;
using HCL_ODA_TestPAD.Functional.Extensions;
using HCL_ODA_TestPAD.ODA.ModelBrowser;
using HCL_ODA_TestPAD.ODA.Draggers.Markups;
using HCL_ODA_TestPAD.ODA.Draggers;
using HCL_ODA_TestPAD.HCL.CadUnits;
using HCL_ODA_TestPAD.Services;
using HCL_ODA_TestPAD.Settings;
using Prism.Events;
using HCL_ODA_TestPAD.Mvvm.Events;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;
using HCL_ODA_TestPAD.ViewModels.Base;
using HCL_ODA_TestPAD.Utility;

namespace HCL_ODA_TestPAD.UserControls;

/// <summary>
/// Interaction logic for CadImageViewControl.xaml
/// </summary>
public partial class DefaultCadImageViewControl : ICadImageViewControl
{
    private readonly HclCadImageViewModel _vmAdapter;
    public HclCadImageViewModel Adapter => _vmAdapter;
    public MainWindowViewModel VM { get; set; }

    public DefaultCadImageViewControl(
        MainWindowViewModel vm,
        IEventAggregator eventAggregator,
        IMessageDialogService messageDialogService,
        IConsoleService consoleService,
        ISettingsProvider settingsProvider)
    {
        InitializeComponent();
        VM = vm;
        _vmAdapter = new HclCadImageViewModel(
            eventAggregator, messageDialogService, consoleService,settingsProvider);
        IsVisibleChanged += VisibilityChanged;
    }

    private void VisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        var visible = (bool)e.NewValue;
        if (visible)
        {
            _vmAdapter.ViewControl = this;
            _vmAdapter.InitViewModel();
        }
        _vmAdapter.VisibilityChanged((bool)e.NewValue);
    }
    
    public void SetFileLoaded(bool isFileLoaded, string filePath)
    {
        VM.FileIsExist = isFileLoaded;
        if (isFileLoaded)
        {
            VM.AppMainWindow.Title = AssemblyHelper.GetAppTitle() + $" - File Loaded : {filePath}";
            VM.PanCommand_Clicked();
        }
        else
        {
            VM.AppMainWindow.Title = AssemblyHelper.GetAppTitle();
        }
    }

    public void InvalidateControl()
    {
        InvalidateVisual();
    }

    public void SetImageSource(WriteableBitmap writableBitmap)
    {
        CadWritableImage.Source = writableBitmap;
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        _vmAdapter.Update();
    }
    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        _vmAdapter.RenderSizeChanged(sizeInfo);
    }
    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        _vmAdapter.MouseDown(e, e.GetPosition(this));
    }

    protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
    {
        _vmAdapter.MouseMove(e, e.GetPosition(this));
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        _vmAdapter.MouseUp(e);
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        _vmAdapter.MouseWheel(e);
    }
    public void Pan()
    {
        _vmAdapter.Pan();
    }

    public void Orbit()
    {
        _vmAdapter.Orbit();
    }

    public void SetRenderModeButton(OdTvGsView.RenderMode mode)
    {
        VM.SetRenderModeButton(mode);
    }
}

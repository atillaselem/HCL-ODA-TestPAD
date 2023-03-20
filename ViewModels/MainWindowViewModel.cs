using Microsoft.Win32;
using System;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows;
using Teigha.Core;
using Teigha.Visualize;
using System.IO;
using HCL_ODA_TestPAD.Mvvm.Commands;
using HCL_ODA_TestPAD.ODA.ModelBrowser;
using HCL_ODA_TestPAD.Views;
using HCL_ODA_TestPAD.Settings;
using HCL_ODA_TestPAD.UserControls;
using HCL_ODA_TestPAD.Mvvm;
using HCL_ODA_TestPAD.ViewModels.Base;
using System.Windows.Forms.Integration;

namespace HCL_ODA_TestPAD.ViewModels;

public class MainWindowViewModel : BindableBase
{
    private readonly IServiceFactory _serviceFactory;
    public AppStatusBarViewModel AppStatusBarViewModel { get; set; }
    public MainWindowViewModel(IServiceFactory serviceFactory,
        AppStatusBarViewModel appStatusBarViewModel)
    {
        _serviceFactory = serviceFactory;
        AppStatusBarViewModel = appStatusBarViewModel;
    }

    //private MemoryManager MM = MemoryManager.GetMemoryManager();

    private bool _fileIsExist = false;
    public bool FileIsExist
    {
        get { return _fileIsExist; }
        set
        {
            _fileIsExist = value;
            AppMainWindow.ActiveRect.Visibility = _fileIsExist ? Visibility.Visible : Visibility.Collapsed;
            if (_fileIsExist)
            {
                NavigationMenuCommand_Clicked();
            }

            //if (TvObjectExplorer == null)
            //    TvObjectExplorer = MainWindow.ModelBrowser;
            //if (TvPropertiesPalette == null)
            //    TvPropertiesPalette = MainWindow.PropertiesPalette;

        }
    }

    private bool _isNavigation = false;
    public bool IsNavigation
    {
        get { return _isNavigation; }
        set
        {
            _isNavigation = value;
            AppMainWindow.NavigationPanel.Visibility = _isNavigation ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private bool _isView;
    public bool IsView
    {
        get { return _isView; }
        set
        {
            _isView = value;
            AppMainWindow.ViewPanel.Visibility = _isView ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private bool _isProjection = false;
    public bool IsProjection
    {
        get { return _isProjection; }
        set
        {
            _isProjection = value;
            AppMainWindow.ProjectionPanel.Visibility = _isProjection ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private bool _isStyle = false;
    public bool IsStyle
    {
        get { return _isStyle; }
        set
        {
            _isStyle = value;
            AppMainWindow.StylePanel.Visibility = _isStyle ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private bool _isDrawing = false;
    public bool IsDrawing
    {
        get { return _isDrawing; }
        set
        {
            _isDrawing = value;
            AppMainWindow.DrawingPanel.Visibility = _isDrawing ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private bool _isMarkup;
    public bool IsMarkup
    {
        get { return _isMarkup; }
        set
        {
            _isMarkup = value;
            AppMainWindow.MarkupPanel.Visibility = _isMarkup ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private bool _isRegen;
    public bool IsRegen
    {
        get { return _isRegen; }
        set
        {
            _isRegen = value;
            AppMainWindow.RegenPanel.Visibility = _isRegen ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private bool _isSectioning;
    public bool IsSectioning
    {
        get { return _isSectioning; }
        set
        {
            _isSectioning = value;
            AppMainWindow.SectioningPanel.Visibility = _isSectioning ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private bool _isPanels;
    public bool IsPanels
    {
        get { return _isPanels; }
        set
        {
            _isPanels = value;
            AppMainWindow.PanelsPanel.Visibility = _isPanels ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    public MainWindow AppMainWindow { get; set; }

    private ResourceDictionary _resources = App.Current.Resources;

    public double CurrentPropertiesWidth = 230;

    private static int _numSaveFile = 0;

    [Flags]
    public enum AppearanceOptions
    {
        FPSEnabled = 1,
        WCSEnabled = 2,
        UseAnimation = 4,
        ViewCubeEnabled = 8
    }

    private AppearanceOptions _appearanceOpt = AppearanceOptions.UseAnimation | AppearanceOptions.ViewCubeEnabled;

    public AppearanceOptions AppearanceOpt
    {
        get { return _appearanceOpt; }
        set
        {
            _appearanceOpt = value;
            if (_hclGLES2_Control != null)
            {
                _hclGLES2_Control.OnOffViewCube((_appearanceOpt & AppearanceOptions.ViewCubeEnabled) == AppearanceOptions.ViewCubeEnabled);
                _hclGLES2_Control.OnOffFPS((_appearanceOpt & AppearanceOptions.FPSEnabled) == AppearanceOptions.FPSEnabled);
                _hclGLES2_Control.OnOffWCS((_appearanceOpt & AppearanceOptions.WCSEnabled) == AppearanceOptions.WCSEnabled);
                _hclGLES2_Control.OnOffAnimation((_appearanceOpt & AppearanceOptions.UseAnimation) == AppearanceOptions.UseAnimation);
            }
        }
    }

    private double _zoomStep = 1.1;
    public double ZoomStep
    {
        get { return _zoomStep; }
        set
        {
            _zoomStep = value;
            if (_hclGLES2_Control != null)
                _hclGLES2_Control.SetZoomStep(_zoomStep);
        }
    }

    private IOpenGLES2Control _hclGLES2_Control;
    public IOpenGLES2Control WpfView
    {
        get { return _hclGLES2_Control; }
    }

    private WindowsFormsHost _winHost = null;
    public void AddView(bool addView = false)
    {
        if(_serviceFactory.AppSettings.RenderDevice == RenderDevice.OpenGL_Bitmap)
        {
            _hclGLES2_Control = new DefaultCadImageViewControl(
                this, _serviceFactory);
            _hclGLES2_Control.AddDefaultViewOnLoad = addView;
            AppMainWindow.RenderArea.Children.Add((DefaultCadImageViewControl)_hclGLES2_Control);
            //_hclGLES2_Control.ShowCustomModels();
        }
        else
        {
            _hclGLES2_Control = new WinFormsCadImageViewControl(
                this, _serviceFactory);
            _winHost = new WindowsFormsHost();
            _winHost.Child = (WinFormsCadImageViewControl)_hclGLES2_Control;
            AppMainWindow.RenderArea.Children.Add(_winHost);
        }

    }

    internal void ClearRenderArea()
    {
        if (_hclGLES2_Control == null) return;
        if (AppMainWindow.RenderArea.Children.Count > 0)
        {
            AppMainWindow.RenderArea.Children.RemoveAt(AppMainWindow.RenderArea.Children.Count - 1);
        }
        _hclGLES2_Control = null;
        FileIsExist = false;
    }

    #region Appearance commands

    private void PlayNavRectAnimation(int right)
    {
        var animation = new ThicknessAnimation();
        animation.From = AppMainWindow.ActiveRect.Margin;
        animation.To = new Thickness(AppMainWindow.ActiveRect.Margin.Left, AppMainWindow.ActiveRect.Margin.Top, right, AppMainWindow.ActiveRect.Margin.Bottom);
        animation.Duration = TimeSpan.FromSeconds(0.15);
        AppMainWindow.ActiveRect.BeginAnimation(FrameworkElement.MarginProperty, animation);
    }

    private void ResetMenuFlags()
    {
        IsNavigation = false;
        IsView = false;
        IsProjection = false;
        IsStyle = false;
        IsRegen = false;
        IsDrawing = false;
        IsMarkup = false;
        IsSectioning = false;
        IsPanels = false;
        //if (_hclGLES2_Control != null)
        //    _hclGLES2_Control.OnAppearSectioningPanel(false);
    }

    // navigation menu switched command
    private RelayCommand _navMenuCommand;
    public RelayCommand NavigationMenuCommand
    {
        get { return _navMenuCommand ?? (_navMenuCommand = new RelayCommand(param => NavigationMenuCommand_Clicked(), param => FileIsExist)); }
    }

    private void NavigationMenuCommand_Clicked()
    {
        ResetMenuFlags();
        IsNavigation = true;
        PlayNavRectAnimation(600);
    }

    // View menu switched command
    private RelayCommand _viewmenuCommand;
    public RelayCommand ViewMenuCommand
    {
        get { return _viewmenuCommand ?? (_viewmenuCommand = new RelayCommand(param => ViewMenuCommand_Clicked(), param => FileIsExist)); }
    }

    private void ViewMenuCommand_Clicked()
    {
        ResetMenuFlags();
        IsView = true;
        PlayNavRectAnimation(500);
    }

    // Projection menu switched command
    private RelayCommand _projMenuCommand;
    public RelayCommand ProjMenuCommand
    {
        get { return _projMenuCommand ?? (_projMenuCommand = new RelayCommand(param => ProjMenuCommand_Clicked(), param => FileIsExist)); }
    }

    private void ProjMenuCommand_Clicked()
    {
        ResetMenuFlags();
        IsProjection = true;
        PlayNavRectAnimation(400);
    }

    // Style menu switched command
    private RelayCommand _styleMenuCommand;
    public RelayCommand StyleMenuCommand
    {
        get { return _styleMenuCommand ?? (_styleMenuCommand = new RelayCommand(param => StyleMenuCommand_Clicked(), param => FileIsExist)); }
    }

    private void StyleMenuCommand_Clicked()
    {
        ResetMenuFlags();
        IsStyle = true;
        PlayNavRectAnimation(300);
    }

    // Markup menu switched command
    private RelayCommand _drawingMenuCommand;
    public RelayCommand DrawingMenuCommand
    {
        get { return _drawingMenuCommand ?? (_drawingMenuCommand = new RelayCommand(param => DrawingMenuCommand_Clicked(), param => FileIsExist)); }
    }

    private void DrawingMenuCommand_Clicked()
    {
        ResetMenuFlags();
        IsDrawing = true;
        PlayNavRectAnimation(0);
    }

    // Markup menu switched command
    private RelayCommand _markupMenuCommand;
    public RelayCommand MarkupMenuCommand
    {
        get { return _markupMenuCommand ?? (_markupMenuCommand = new RelayCommand(param => MarkupMenuCommand_Clicked(), param => FileIsExist)); }
    }

    private void MarkupMenuCommand_Clicked()
    {
        ResetMenuFlags();
        IsMarkup = true;
        PlayNavRectAnimation(100);
    }

    // Regen menu switched command
    private RelayCommand _regenMenuCommand;
    public RelayCommand RegenMenuCommand
    {
        get { return _regenMenuCommand ?? (_regenMenuCommand = new RelayCommand(param => RegenMenuCommand_Clicked(), param => FileIsExist)); }
    }

    private void RegenMenuCommand_Clicked()
    {
        ResetMenuFlags();
        IsRegen = true;
        PlayNavRectAnimation(200);
    }

    // Regen menu switched command
    private RelayCommand _sectioningMenuCommand;
    public RelayCommand SectioningMenuCommand
    {
        get { return _sectioningMenuCommand ?? (_sectioningMenuCommand = new RelayCommand(param => SectioningMenuCommand_Clicked(), param => FileIsExist)); }
    }

    private void SectioningMenuCommand_Clicked()
    {
        ResetMenuFlags();
        IsSectioning = true;
        PlayNavRectAnimation(-100);
        if (_hclGLES2_Control != null)
            _hclGLES2_Control.OnAppearSectioningPanel(true);
    }
    #endregion

    #region Panels Menu
    // Panels menu switched command
    private RelayCommand _panelsMenuCommand;
    public RelayCommand PanelsMenuCommand
    {
        get { return _panelsMenuCommand ?? (_panelsMenuCommand = new RelayCommand(param => PanelsMenuCommand_Clicked(), param => FileIsExist)); }
    }

    private void PanelsMenuCommand_Clicked()
    {
        ResetMenuFlags();
        IsPanels = true;
        PlayNavRectAnimation(-200);
    }

    // OnOffTreeBrowserCommand command
    private RelayCommand _OnOffTreeBrowserCommand;
    public RelayCommand OnOffTreeBrowserCommand
    {
        get
        {
            if (_OnOffTreeBrowserCommand != null)
                return _OnOffTreeBrowserCommand;
            else
                return (_OnOffTreeBrowserCommand = new RelayCommand(param => OnOffModelBrowser(param)));
        }
    }

    private void OnOffModelBrowser(object param)
    {
        bool isEnabled = (bool)param;
        if (isEnabled == true && WpfView != null && WpfView.TvDatabaseId != null)
            //MainWindow.ModelBrowser.Initialize(WpfView.TvDatabaseId, WpfView);
        if (isEnabled == false)
        {
            //MainWindow.TreeColumn.MinWidth = 0;
            //MainWindow.TreeColumn.Width = GridLength.Auto;
            //MainWindow.MainGrid.Children.Remove(MainWindow.TreeGrid);
            //MainWindow.MainGrid.Children.Remove(MainWindow.TreeSplitter);
        }
        else
        {
            //MainWindow.TreeColumn.MinWidth = 220;
            //MainWindow.TreeColumn.Width = GridLength.Auto;
            //MainWindow.MainGrid.Children.Add(MainWindow.TreeGrid);
            //MainWindow.MainGrid.Children.Add(MainWindow.TreeSplitter);
        }
    }

    private RelayCommand _onOffPropertiesPaletteCommand;
    public RelayCommand OnOffPropertiesPaletteCommand
    {
        get
        {
            if (_onOffPropertiesPaletteCommand != null)
                return _onOffPropertiesPaletteCommand;
            else
                return (_onOffPropertiesPaletteCommand = new RelayCommand(param => OnOffPropertiesPalette(param)));
        }
    }

    private void OnOffPropertiesPalette(object param)
    {
        bool isEnabled = (bool)param;
        if (isEnabled == false)
        {
            //MainWindow.PropertiesColumn.MinWidth = 0;
            //MainWindow.PropertiesColumn.Width = GridLength.Auto;
            //MainWindow.MainGrid.Children.Remove(MainWindow.PropertiesGrid);
            //MainWindow.MainGrid.Children.Remove(MainWindow.PropertiesSplitter);
        }
        else
        {
            //MainWindow.PropertiesColumn.MinWidth = 250;
            //MainWindow.PropertiesColumn.Width = GridLength.Auto;
            //MainWindow.MainGrid.Children.Add(MainWindow.PropertiesGrid);
            //MainWindow.MainGrid.Children.Add(MainWindow.PropertiesSplitter);
        }
    }

    #endregion


    #region File commands
    // AddView command
    private RelayCommand _addViewCommand;
    public RelayCommand AddViewCommand
    {
        get
        {
            if (_addViewCommand != null)
                return _addViewCommand;
            else
                return (_addViewCommand = new RelayCommand(param => AddViewCommand_Clicked()));
        }
    }

    private void AddViewCommand_Clicked()
    {
        ClearRenderArea();
        if (_hclGLES2_Control == null)
            AddView(true);
    }

    // ClearView command
    private RelayCommand _clearViewCommand;
    public RelayCommand ClearViewCommand
    {
        get
        {
            if (_clearViewCommand != null)
                return _clearViewCommand;
            else
                return (_clearViewCommand = new RelayCommand(param => ClearViewCommand_Clicked()));
        }
    }
    private void ClearViewCommand_Clicked()
    {
        ClearRenderArea();
        if (_hclGLES2_Control == null)
            AddView(false);
    }
    // Open command
    private RelayCommand _openCommand;
    public RelayCommand OpenCommand
    {
        get
        {
            if (_openCommand != null)
                return _openCommand;
            else
                return (_openCommand = new RelayCommand(param => OpenCommand_Clicked()));
        }
    }
    // Settings command
    private RelayCommand _settingsCommand;
    public RelayCommand SettingsCommand
    {
        get
        {
            if (_settingsCommand != null)
                return _settingsCommand;
            else
                return (_settingsCommand = new RelayCommand(param => SettingsCommand_Clicked()));
        }
    }
    public void SaveSettings()
    {
        _serviceFactory.SettingsSrv.SaveSettings(_serviceFactory.AppSettings);
    }
    private void SettingsCommand_Clicked()
    {
        var settingsView = new TestPADSettingsView();
        var settingsViewModel = new TestPADSettingsViewModel(_serviceFactory);
        settingsViewModel.TestPadSettings = new TestPADSettings(_serviceFactory);
        settingsView.DataContext = settingsViewModel;
        Window window = new Window
        {
            Title = "TestPAD Settings Panel",
            Content = settingsView,
            SizeToContent = SizeToContent.WidthAndHeight,
            ResizeMode = ResizeMode.NoResize,
            WindowStartupLocation = WindowStartupLocation.CenterScreen
        };
        window.ShowDialog();
    }
    private void OpenCommand_Clicked()
    {
        OpenFileDialog dlg = new OpenFileDialog();
        dlg.Filter = "Open Design Visualize Stream|*.vsf|" +
                     "VSFX files|*.vsfx|" +
                     "DWG files|*.dwg|" +
                     "DXF files|*.dxf|" +
                     "IFC files|*.ifc|" +
                     "OBJ files|*.obj|" +
                     "STL files|*.stl|" +
                     "All Supported files|*.vsf;*.vsfx;*.dwg;*.dxf;*.ifc;*.obj;*.stl";

        RegistryKey key = Registry.CurrentUser.OpenSubKey("WpfVisualizeViewer_OpenDialogIndex", true);
        if (key == null)
        {
            key = Registry.CurrentUser.CreateSubKey("WpfVisualizeViewer_OpenDialogIndex");
            key.SetValue("Index", 1);
        }
        dlg.FilterIndex = (int)key.GetValue("Index");

        if (dlg.ShowDialog() != true)
            return;

        key.SetValue("Index", dlg.FilterIndex);
        key.Close();

        ClearRenderArea();
        if (_hclGLES2_Control == null)
            AddView();
        //_hclGLES2_Control?.ClearDevices();
        //if (_hclGLES2_Control == null)
        //AddView(false);

        _hclGLES2_Control.LoadFile(dlg.FileName);
        //_hclGLES2_Control.OnOffFPS(true);
        _hclGLES2_Control.ShowCustomModels();
        //UncheckDraggersBtns();
    }

    // New command
    private RelayCommand _newCommand;
    public RelayCommand NewCommand
    {
        get
        {
            if (_newCommand != null)
                return _newCommand;
            else
                return (_newCommand = new RelayCommand(param => NewCommand_Clicked()));
        }
    }

    private void NewCommand_Clicked()
    {
        if (_hclGLES2_Control == null)
            AddView();
        //_hclGLES2_Control.CreateNewFile();
        //_hclGLES2_Control.Focus();
    }

    // Save command
    private RelayCommand _saveCommand;
    public RelayCommand SaveCommand
    {
        get
        {
            if (_saveCommand != null)
                return _saveCommand;
            else
                return (_saveCommand = new RelayCommand(param => SaveCommand_Clicked(), param => FileIsExist));
        }
    }

    private void SaveCommand_Clicked()
    {
        if (_hclGLES2_Control.FilePath.Length > 0 && File.Exists(_hclGLES2_Control.FilePath) && System.IO.Path.GetExtension(WpfView.FilePath) == ".vsf")
            _hclGLES2_Control.SaveFile(_hclGLES2_Control.FilePath);
        else
            SaveAsCommand_Clicked();
    }

    // Save as command
    private RelayCommand _saveAsCommand;
    public RelayCommand SaveAsCommand
    {
        get { return _saveAsCommand ?? (_saveAsCommand = new RelayCommand(param => SaveAsCommand_Clicked(), param => FileIsExist)); }
    }

    private void SaveAsCommand_Clicked()
    {
        SaveFileDialog dlg = new SaveFileDialog
        {
            DefaultExt = ".vsf",
            Filter = "Open Design Visualize Stream|*.vsf"
        };

        if (WpfView.FilePath.Length > 0)
        {
            string name = Path.GetFileName(WpfView.FilePath);
            string ext = Path.GetExtension(WpfView.FilePath);
            dlg.FileName = name.Remove(name.Length - ext.Length);
        }
        else
            dlg.FileName = "VisualizeStream_" + _numSaveFile++;

        if (dlg.ShowDialog() != true)
            return;

        if (dlg.FileName.Length > 0)
            WpfView.SaveFile(dlg.FileName);
    }

    // Export to pdf command
    private RelayCommand _export2dPdfCommand;
    public RelayCommand Export2dPdfCommand
    {
        get { return _export2dPdfCommand ?? (_export2dPdfCommand = new RelayCommand(param => Export2dCommand_Clicked(), param => FileIsExist)); }
    }

    private void Export2dCommand_Clicked()
    {
        ExportToPdf(true);
    }

    private RelayCommand _export3dPdfCommand;
    public RelayCommand Export3dPdfCommand
    {
        get { return _export3dPdfCommand ?? (_export3dPdfCommand = new RelayCommand(param => Export3dCommand_Clicked(), param => FileIsExist)); }
    }

    private void Export3dCommand_Clicked()
    {
        ExportToPdf(false);
    }

    private void ExportToPdf(bool is2D)
    {
        SaveFileDialog dlg = new SaveFileDialog
        {
            Filter = "PDF Files(*.pdf)|*.pdf"
        };

        string fileName = Path.GetFileName(_hclGLES2_Control.FilePath);
        if (fileName != null)
        {
            fileName = fileName.Remove(fileName.Length - Path.GetExtension(fileName).Length);
            dlg.FileName = fileName;
        }
        else
            return;

        if (dlg.ShowDialog() != true)
            return;

        if (dlg.FileName.Length > 0)
            _hclGLES2_Control.ExportToPdf(dlg.FileName, is2D);

        //_hclGLES2_Control.Focus();
    }

    #endregion

    #region Navigation commands

    public void UncheckDraggersBtns()
    {
        AppMainWindow.PanBtn.IsChecked = false;
        AppMainWindow.OrbitBtn.IsChecked = false;
    }

    // pan command
    private RelayCommand _panCommand;
    public RelayCommand PanCommand
    {
        get { return _panCommand ?? (_panCommand = new RelayCommand(param => PanCommand_Clicked(), param => FileIsExist)); }
    }

    public void PanCommand_Clicked()
    {
        AppMainWindow.OrbitBtn.IsChecked = false;
        AppMainWindow.ZoomToAreaBtn.IsChecked = false;
        AppMainWindow.PanBtn.IsChecked = true;
        _hclGLES2_Control.ZoomToArea(AppMainWindow.ZoomToAreaBtn.IsChecked == true);
        //then raise a click evt
        //MainWindow.PanBtn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        if (AppMainWindow.PanBtn.IsChecked == true)
            _hclGLES2_Control.Pan();
        else
            _hclGLES2_Control.FinishDragger();


    }

    // orbit command
    private RelayCommand _orbitCommand;
    public RelayCommand OrbitCommand
    {
        get { return _orbitCommand ??= new RelayCommand(param => OrbitCommand_Clicked(), param => FileIsExist); }
    }

    public void OrbitCommand_Clicked()
    {
        AppMainWindow.PanBtn.IsChecked = false;
        AppMainWindow.ZoomToAreaBtn.IsChecked = false;
        _hclGLES2_Control.ZoomToArea(AppMainWindow.ZoomToAreaBtn.IsChecked == true);
        if (AppMainWindow.OrbitBtn.IsChecked == true)
            _hclGLES2_Control.Orbit();
        else
            _hclGLES2_Control.FinishDragger();
    }

    // zoom in command
    private RelayCommand _zoomCommand;
    public RelayCommand ZoomCommand
    {
        get { return _zoomCommand ??= new RelayCommand(param => ZoomCommand_Clicked(param), param => FileIsExist); }
    }

    private void ZoomCommand_Clicked(object param)
    {
        switch (param.ToString())
        {
            case "Zoom In":
                _hclGLES2_Control.Zoom(ZoomType.ZoomIn);
                break;
            case "Zoom Out":
                _hclGLES2_Control.Zoom(ZoomType.ZoomOut);
                break;
            case "Zoom Extents":
                _hclGLES2_Control.Zoom(ZoomType.ZoomExtents);
                break;
        }
    }

    // orbit command
    private RelayCommand _zoomToAreaCommand;
    public RelayCommand ZoomToAreaCommand
    {
        get { return _zoomToAreaCommand ??= new RelayCommand(param => ZoomToAreaCommand_Clicked(), param => FileIsExist); }
    }

    public void ZoomToAreaCommand_Clicked()
    {
        AppMainWindow.PanBtn.IsChecked = false;
        AppMainWindow.OrbitBtn.IsChecked = false;
        _hclGLES2_Control.ZoomToArea(AppMainWindow.ZoomToAreaBtn.IsChecked == true);
    }
    #endregion

    #region Drawing commands

    // rect markup
    private RelayCommand _drawingCommand;
    public RelayCommand DrawingCommand
    {
        get { return _drawingCommand ?? (_drawingCommand = new RelayCommand(param => DrawingCommand_Clicked(param), param => FileIsExist)); }
    }

    private void DrawingCommand_Clicked(object param)
    {
        _hclGLES2_Control.DrawGeometry(param.ToString());
    }

    #endregion

    #region Markups commands

    // rect markup
    private RelayCommand _markupCommand;
    public RelayCommand MarkupCommand
    {
        get { return _markupCommand; }
        //get { return _markupCommand ?? (_markupCommand = new RelayCommand(param => MarkupCommand_Clicked(param), param => FileIsExist)); }
    }

    private void MarkupCommand_Clicked(object param)
    {
        switch (param.ToString())
        {
            case "Rectangle":
                _hclGLES2_Control.DrawRectMarkup();
                break;
            case "Circle":
                _hclGLES2_Control.DrawCircMarkup();
                break;
            case "Handle":
                _hclGLES2_Control.DrawHandleMarkup();
                break;
            case "Cloud":
                _hclGLES2_Control.DrawCloudMarkup();
                break;
            case "Text":
                _hclGLES2_Control.DrawTextMarkup();
                break;
            case "Save":
                _hclGLES2_Control.SaveMarkup();
                break;
            case "Load":
                _hclGLES2_Control.LoadMarkup();
                break;
        }
    }

    #endregion

    #region View settings

    // render mode commands
    private RelayCommand _renderModeCommand;
    public RelayCommand RenderModeCommand
    {
        get { return _renderModeCommand ?? (_renderModeCommand = new RelayCommand(param => RenderModeCommand_Clicked(param), param => FileIsExist)); }
    }

    private void RenderModeCommand_Clicked(object param)
    {
        switch (param.ToString())
        {
            case "2D Wireframe":
                _hclGLES2_Control.SetRenderMode(OdTvGsView.RenderMode.k2DOptimized);
                break;
            case "3D Wireframe":
                _hclGLES2_Control.SetRenderMode(OdTvGsView.RenderMode.kWireframe);
                break;
            case "HiddenLine":
                _hclGLES2_Control.SetRenderMode(OdTvGsView.RenderMode.kHiddenLine);
                break;
            case "Shaded":
                _hclGLES2_Control.SetRenderMode(OdTvGsView.RenderMode.kFlatShaded);
                break;
            case "Gouraud shaded":
                _hclGLES2_Control.SetRenderMode(OdTvGsView.RenderMode.kGouraudShaded);
                break;
            case "Shaded with edges":
                _hclGLES2_Control.SetRenderMode(OdTvGsView.RenderMode.kFlatShadedWithWireframe);
                break;
            case "Gouraud shaded with edges":
                _hclGLES2_Control.SetRenderMode(OdTvGsView.RenderMode.kGouraudShadedWithWireframe);
                break;
        }
        //_hclGLES2_Control.Focus();
    }

    public void SetRenderModeButton(OdTvGsView.RenderMode mode)
    {
        AppMainWindow.Wireframe2DBtn.IsChecked = false;
        AppMainWindow.Wireframe3DBtn.IsChecked = false;
        AppMainWindow.HiddenLineBtn.IsChecked = false;
        AppMainWindow.ShadedBtn.IsChecked = false;
        //MainWindow.GouraudShadedBtn.IsChecked = false;
        AppMainWindow.ShadedWithEdgesBtn.IsChecked = false;
        //MainWindow.GouraudShadedWithEdgesBtn.IsChecked = false;
        switch (mode)
        {
            case OdTvGsView.RenderMode.k2DOptimized:
                AppMainWindow.Wireframe2DBtn.IsChecked = true;
                break;
            case OdTvGsView.RenderMode.kWireframe:
                AppMainWindow.Wireframe3DBtn.IsChecked = true;
                break;
            case OdTvGsView.RenderMode.kHiddenLine:
                AppMainWindow.HiddenLineBtn.IsChecked = true;
                break;
            case OdTvGsView.RenderMode.kFlatShaded:
                AppMainWindow.ShadedBtn.IsChecked = true;
                break;
            //case OdTvGsView.RenderMode.kGouraudShaded:
            //    MainWindow.GouraudShadedBtn.IsChecked = true;
            //    break;
            case OdTvGsView.RenderMode.kFlatShadedWithWireframe:
                AppMainWindow.ShadedWithEdgesBtn.IsChecked = true;
                break;
            //case OdTvGsView.RenderMode.kGouraudShadedWithWireframe:
            //    MainWindow.GouraudShadedWithEdgesBtn.IsChecked = true;
            //    break;
        }
        //_hclGLES2_Control.Focus();
    }

    // background color command
    private RelayCommand _bgCommand;
    public RelayCommand BackgroundCommand
    {
        get { return _bgCommand ?? (_bgCommand = new RelayCommand(param => BackgroundCommand_Clicked(), param => FileIsExist)); }
    }

    private void BackgroundCommand_Clicked()
    {
        //ColorDialog dlg = new ColorDialog();
        //if (dlg.ShowDialog() == DialogResult.OK)
        //    _hclGLES2_Control.SetBackgroundColor(dlg.Color);
        //_hclGLES2_Control.Focus();
    }

    // regen commands
    private RelayCommand _regenCommand;
    public RelayCommand RegenCommand
    {
        get { return _regenCommand ?? (_regenCommand = new RelayCommand(param => RegenCommand_Clicked(param), param => FileIsExist)); }
    }

    private void RegenCommand_Clicked(object param)
    {
        switch (param.ToString())
        {
            case "RegenAll":
                _hclGLES2_Control.Regen(OdTvGsDevice.RegenMode.kRegenAll);
                break;
            case "RegenVisible":
                _hclGLES2_Control.Regen(OdTvGsDevice.RegenMode.kRegenVisible);
                break;
            case "RegenView":
                _hclGLES2_Control.Regen();
                break;
        }
        //_hclGLES2_Control.Focus();
    }

    // View commands
    private RelayCommand _viewCommand;
    public RelayCommand ViewCommand
    {
        get { return _viewCommand ?? (_viewCommand = new RelayCommand(param => ViewCommand_Clicked(param), param => FileIsExist)); }
    }

    private void ViewCommand_Clicked(object param)
    {
        switch (param.ToString())
        {
            case "Top":
                _hclGLES2_Control.Set3DView(OdTvExtendedView.e3DViewType.kTop);
                break;
            case "Bottom":
                _hclGLES2_Control.Set3DView(OdTvExtendedView.e3DViewType.kBottom);
                break;
            case "Left":
                _hclGLES2_Control.Set3DView(OdTvExtendedView.e3DViewType.kLeft);
                break;
            case "Right":
                _hclGLES2_Control.Set3DView(OdTvExtendedView.e3DViewType.kRight);
                break;
            case "Front":
                _hclGLES2_Control.Set3DView(OdTvExtendedView.e3DViewType.kFront);
                break;
            case "Back":
                _hclGLES2_Control.Set3DView(OdTvExtendedView.e3DViewType.kBack);
                break;
            case "SW Isometric":
                _hclGLES2_Control.Set3DView(OdTvExtendedView.e3DViewType.kSW);
                break;
            case "SE Isometric":
                _hclGLES2_Control.Set3DView(OdTvExtendedView.e3DViewType.kSE);
                break;
            case "NE Isometric":
                _hclGLES2_Control.Set3DView(OdTvExtendedView.e3DViewType.kNE);
                break;
            case "NW Isometric":
                _hclGLES2_Control.Set3DView(OdTvExtendedView.e3DViewType.kNW);
                break;
        }
        //_hclGLES2_Control.Focus();
    }

    // Projection commands
    private RelayCommand _projCommand;
    public RelayCommand ProjectionCommand
    {
        get { return _projCommand ?? (_projCommand = new RelayCommand(param => ProjectionCommand_Clicked(param), param => FileIsExist)); }
    }

    private void ProjectionCommand_Clicked(object param)
    {
        switch (param.ToString())
        {
            case "Isometric":
                AppMainWindow.PerspectiveBtn.IsChecked = false;
                AppMainWindow.IsometricBtn.IsChecked = true;
                _hclGLES2_Control.SetProjectionType(OdTvGsView.Projection.kParallel);
                break;
            case "Perspective":
                AppMainWindow.IsometricBtn.IsChecked = false;
                AppMainWindow.PerspectiveBtn.IsChecked = true;
                _hclGLES2_Control.SetProjectionType(OdTvGsView.Projection.kPerspective);
                break;
        }
        //_hclGLES2_Control.Focus();
    }

    #endregion

    #region Sectioning commands

    // regen commands
    private RelayCommand _sectioningCommand;
    public RelayCommand SectioningCommand
    {
        get { return _sectioningCommand ?? (_sectioningCommand = new RelayCommand(param => SectioningCommand_Clicked(param), param => FileIsExist)); }
    }

    private void SectioningCommand_Clicked(object param)
    {
        if (_hclGLES2_Control == null)
            return;
        switch (param.ToString())
        {
            case "CuttingPlaneShow":
                {
                    WpfView.ShowCuttingPlanes();
                    string img = WpfView.SectioningOptions.IsShown ? "CuttingPlane" : "CuttingPlaneOff";
                    AppMainWindow.CuttingPlaneShowImg.Source = System.Windows.Application.Current.Resources[img] as BitmapImage;
                    break;
                }
            case "AddCuttingPlaneX":
                {
                    OdGeVector3d eyeDir = WpfView.GetEyeDirection();
                    double xDot = eyeDir.dotProduct(OdGeVector3d.kXAxis);
                    if (Math.Abs(xDot).Equals(0d))
                        xDot = 1d;

                    OdGeVector3d axis = new OdGeVector3d(OdGeVector3d.kXAxis);
                    axis *= -xDot;
                    axis = axis.normalize();
                    AddCuttingPlane(axis);
                    break;
                }
            case "AddCuttingPlaneY":
                {
                    OdGeVector3d eyeDir = WpfView.GetEyeDirection();
                    double yDot = eyeDir.dotProduct(OdGeVector3d.kYAxis);
                    if (Math.Abs(yDot).Equals(0d))
                        yDot = 1d;

                    OdGeVector3d axis = new OdGeVector3d(OdGeVector3d.kYAxis);
                    axis *= -yDot;
                    axis = axis.normalize();
                    AddCuttingPlane(axis);
                    break;
                }
            case "AddCuttingPlaneZ":
                {
                    OdGeVector3d eyeDir = WpfView.GetEyeDirection();
                    double zDot = eyeDir.dotProduct(OdGeVector3d.kZAxis);
                    if (Math.Abs(zDot).Equals(0d))
                        zDot = 1d;

                    OdGeVector3d axis = new OdGeVector3d(OdGeVector3d.kZAxis);
                    axis *= -zDot;
                    axis = axis.normalize();
                    AddCuttingPlane(axis);
                    break;
                }
            case "FillCuttingPlane":
                {
                    WpfView.ShowSectioningOptions();
                    break;
                }
            case "RemoveCuttingPlane":
                {
                    WpfView.RemoveCuttingPlanes();
                    break;
                }
        }
        //_hclGLES2_Control.Focus();
    }

    private void AddCuttingPlane(OdGeVector3d axis)
    {
        OdTvResult res = OdTvResult.tvOk;
        if (!WpfView.AddCuttingPlane(axis, res))
        {
            if (res == OdTvResult.tvOk)
            {
                //System.Windows.Forms.MessageBox.Show("There are can not be more than " + CadImageViewControl.Impl.OD_TV_CUTTING_PLANE_MAX_NUM + " cutting planes", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return;
            }
        }
    }

    #endregion

    #region ModelBrowserCommads

    private RelayCommand _itemClickCommand;

    public RelayCommand ItemClickCommand
    {
        get { return _itemClickCommand ?? (_itemClickCommand = new RelayCommand(param => ItemClickCommand_Clicked(param))); }
    }

    private void ItemClickCommand_Clicked(object param)
    {
        TvTreeItem itm = param as TvTreeItem;
        if (itm == null || WpfView == null)
            return;
        WpfView.AddBoldItem(itm);
        //TvPropertiesPalette.FillObjectParameters(itm);

        if (itm.NodeData.Type == TvBrowserItemType.Entity)
        {
            OdTvEntityId enId = itm.NodeData.EntityId;
            if (WpfView != null && (enId.getType() == OdTvEntityId.EntityTypes.kEntity
                                    || enId.getType() == OdTvEntityId.EntityTypes.kInsert))
            {
                WpfView.AddEntityToSet(enId);
            }

        }
    }

    #endregion

    #region ZoomToArea Link To Dependency Property
    private bool _zoomToAreaProperty;
    public bool IsZoomToAreaEnabled
    {
        get => _zoomToAreaProperty;
        set => SetProperty(ref _zoomToAreaProperty, value);
    }
    #endregion

}


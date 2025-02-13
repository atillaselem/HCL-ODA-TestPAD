using HCL_ODA_TestPAD.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HCL_ODA_TestPAD.Settings;
using HCL_ODA_TestPAD.ViewModels.Base;
using HCL_ODA_TestPAD.ODA.Draggers;
using HCL_ODA_TestPAD.ODA.ModelBrowser;
using ODA.Kernel.TD_RootIntegrated;
using ODA.Visualize.TV_Visualize;
using ODA.Visualize.TV_VisualizeTools;
using System.IO;
using System.Threading;
using HCL_ODA_TestPAD.Mvvm.Events;
using HCL_ODA_TestPAD.HCL.UserActions.States;
using HCL_ODA_TestPAD.Views;

namespace HCL_ODA_TestPAD.UserControls;

/// <summary>
/// Interaction logic for CadImageViewControl.xaml
/// </summary>
public partial class DefaultCadImageViewControl : IOpenGles2Control, ICadImageViewControl
{
    private readonly IServiceFactory _serviceFactory;

    private readonly HclCadImageViewModel _vmAdapter;
    //public HclCadImageViewModel Adapter => _vmAdapter;
    public MainWindowViewModel MainWindowVm { get; set; }
    public string FilePath => _vmAdapter.FilePath;
    private IUserActionState _userActionState;
    private ActionZoomToScale _actionZoomToScale;
    private readonly IUserActionManager _userActionManager;
    private readonly OverlayViewModel _overlayViewModel;
    public bool AddDefaultViewOnLoad
    {
        get => _vmAdapter.AddDefaultViewOnLoad;
        set
        {
            _vmAdapter.AddDefaultViewOnLoad = value;
        }
    }
    public OdTvSectioningOptions SectioningOptions { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public OdTvDatabaseId TvDatabaseId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    
    public DefaultCadImageViewControl(MainWindowViewModel mainWindowVm, IServiceFactory serviceFactory, OverlayViewModel overlayViewModel)
    {
        InitializeComponent();
        MainWindowVm = mainWindowVm;
        _serviceFactory = serviceFactory;
        _vmAdapter = new HclCadImageViewModel(serviceFactory);
        IsVisibleChanged += VisibilityChanged;
        _userActionManager = new UserActionManager(serviceFactory.EventSrv, 
            () => dragSelectionCanvas, 
            () => dragSelectionBorder,
            this,
            serviceFactory.AppSettings,
            _vmAdapter);
        _userActionState = new ActionIdle(_userActionManager);
        _overlayViewModel = overlayViewModel;
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

    public void SetFileLoaded(bool isFileLoaded, string filePath, Action<string> emitEvent, bool isCancelled = false)
    {
        MainWindowVm.FileIsExist = isFileLoaded;
        if (isFileLoaded)
        {
            MainWindowVm.PanCommand_Clicked();
            emitEvent?.Invoke($"File : [{filePath}] loaded successfully.");
            _vmAdapter.ShowCustomModels();
            _actionZoomToScale = new ActionZoomToScale(_userActionManager);
        }
        else
        {
            MainWindowVm.AppMainWindow.HclToolStationBtn.IsChecked = false;
            MainWindowVm.AppMainWindow.HclToolPrismBtn.IsChecked = false;
            MainWindowVm.AppMainWindow.HclToolPointsBtn.IsChecked = false;
            emitEvent?.Invoke($"File : [{filePath}] {(isCancelled ? "cancelled" : "unloaded")}.");
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

    #region Action State Transitions
    public IUserActionState TransitState(UserInteraction userInteraction) =>
    _userActionState = _userActionState.DoStateTransition(userInteraction);


    private void UserInteractionChanged(string property)
    {
        TransitState(
            property switch
            {
                "Pan" => UserInteraction.Panning,
                "Orbit" => UserInteraction.Orbiting,
                "ZoomToArea" => UserInteraction.ZoomToArea,
                _ => _userActionState.ActiveAction
            }
        );
    }
    #endregion

    #region Mouse & Touch Events
    /// <summary>
    /// WPF converts Touch events to Mouse events if they are not handled. 
    /// </summary>
    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        _userActionState.ExecuteMouseTouchDown(e, this);
        if (MainWindowVm.AppMainWindow.HclToolStationBtn.IsChecked != null && MainWindowVm.AppMainWindow.HclToolStationBtn.IsChecked.Value && (e.MiddleButton == MouseButtonState.Pressed))
        {
            _vmAdapter.ChangeStationLocation(e.GetPosition(this));
        }
        else if (MainWindowVm.AppMainWindow.HclToolPrismBtn.IsChecked != null && MainWindowVm.AppMainWindow.HclToolPrismBtn.IsChecked.Value && (e.RightButton == MouseButtonState.Pressed))
        {
            _vmAdapter.ChangePrismLocation(e.GetPosition(this));
        }
        if (_userActionState.ActiveAction is UserInteraction.Orbiting or UserInteraction.ZoomToScale && _serviceFactory.AppSettings.HidePointTextTransformation)
        {
            _vmAdapter.ToggleTextTransformation(false);
        }

        ToggleAsyncPbo(true);
    }

    private void ToggleAsyncPbo(bool enable)
    {
        using var dev = _vmAdapter.TvGsDeviceId?.openObject(OdTv_OpenMode.kForWrite);
        dev?.setOption(OdTvGsDevice_Options.kAsyncReadback, enable ? 2 : 0);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        if (_userActionState.ActiveAction == UserInteraction.ZoomToArea)
        {
            using var _ = _vmAdapter.UpdateStatusBarCoordinates(e.GetPosition(this));
        }
        else if (e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }
        _userActionState.ExecuteMouseTouchMove(e, this);
    }
    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        _userActionState.ExecuteMouseTouchUp(e, this);
        ToggleAsyncPbo(false);
        _vmAdapter.UpdateHclZoomTransformations();
        if (_userActionState.ActiveAction is UserInteraction.Orbiting or UserInteraction.ZoomToScale && _serviceFactory.AppSettings.HidePointTextTransformation)
        {
            _vmAdapter.ToggleTextTransformation(true);
        }
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        //Return back to active user action state before Wheeling executes
        _userActionState = _userActionState
                           .DoStateTransition(UserInteraction.Wheeling)
                           .ExecuteWheeling(e, _userActionState.ActiveAction, () => MessageBox.Show("Zoom Not Possible"));
    }
    #endregion

    #region Overriden Touch Events
    protected override void OnTouchDown(TouchEventArgs e)
    {
        _actionZoomToScale.ExecuteMouseTouchDown(e, this);
    }
    protected override void OnTouchMove(TouchEventArgs e)
    {
        _actionZoomToScale.ExecuteMouseTouchMove(e, this);
        _vmAdapter.UpdateHclZoomTransformations(isPinchZoom: true);
    }
    protected override void OnTouchUp(TouchEventArgs e)
    {
        _actionZoomToScale.ExecuteMouseTouchUp(e, this);
    }
    protected override void OnTouchLeave(TouchEventArgs e)
    {
        _actionZoomToScale!.ExecuteTouchLeave(e, this);
        _vmAdapter.UpdateHclZoomTransformations();
    }
    #endregion

    public void Pan()
    {
        _vmAdapter.Pan();
        UserInteractionChanged("Pan");
    }

    public void Orbit()
    {
        _vmAdapter.Orbit();
        UserInteractionChanged("Orbit");
    }

    public void ZoomToArea(bool enable)
    {
        UserInteractionChanged("ZoomToArea");
    }

    public void SetRenderModeButton(OdTvGsView_RenderMode mode)
    {
        MainWindowVm.SetRenderModeButton(mode);
    }

    public void DrawCircMarkup()
    {
        _vmAdapter.DrawCircMarkup();
    }

    public void DrawCloudMarkup()
    {
        _vmAdapter.DrawCloudMarkup();
    }

    public void DrawGeometry(string type)
    {
        _vmAdapter.DrawGeometry(type);
    }

    public void DrawHandleMarkup()
    {
        _vmAdapter.DrawHandleMarkup();
    }

    public void DrawRectMarkup()
    {
        _vmAdapter.DrawRectMarkup();
    }

    public void DrawTextMarkup()
    {
        _vmAdapter.DrawTextMarkup();
    }

    public void ExportToPdf(string fileName, bool is2D = true)
    {
        _vmAdapter.ExportToPdf(fileName, is2D);
    }
    public void SaveAsVsfx(string fileName)
    {
        _vmAdapter.SaveFileAsVsfx(fileName);
    }
    public void FinishDragger()
    {
        _vmAdapter.FinishDragger();
    }

    public void LoadMarkup()
    {
        _vmAdapter.LoadMarkup();
    }

    public void OnAppearSectioningPanel(bool bAppear)
    {
        _vmAdapter.OnAppearSectioningPanel(bAppear);
    }

    public void OnOffAnimation(bool bEnable)
    {
        _vmAdapter.OnOffAnimation(bEnable);
    }

    public void OnOffFPS(bool bEnable)
    {
        _vmAdapter.OnOffFPS(bEnable);
    }

    public void OnOffViewCube(bool bEnable)
    {
        _vmAdapter.OnOffViewCube(bEnable);
    }

    public void OnOffWCS(bool bEnable)
    {
        _vmAdapter.OnOffWCS(bEnable);
    }

    public void Regen()
    {
        _vmAdapter.Regen();
    }

    public void Regen(OdTvGsDevice_RegenMode rm)
    {
        _vmAdapter.Regen(rm);
    }

    public void SaveFile(string filePath)
    {
        _vmAdapter.SaveFile(filePath);
    }

    public void SaveMarkup()
    {
        _vmAdapter.SaveMarkup();
    }

    public void Set3DView(OdTvExtendedView_e3DViewType type)
    {
        _vmAdapter.Set3DView(type);
    }

    public void SetProjectionType(OdTvGsView_Projection projection)
    {
        _vmAdapter.SetProjectionType(projection);
    }

    public void SetRenderMode(OdTvGsView_RenderMode renderMode)
    {
        _vmAdapter.SetRenderMode(renderMode);
    }

    public void SetZoomStep(double dValue)
    {
        _vmAdapter.SetZoomStep(dValue);
    }

    public void Zoom(ZoomType type)
    {
        _vmAdapter.Zoom(type);
    }

    public void AddEntityToSet(OdTvEntityId enId)
    {
        _vmAdapter.AddEntityToSet(enId);
    }

    public void AddBoldItem(TvTreeItem node)
    {
        _vmAdapter.AddBoldItem(node);
    }

    public bool AddCuttingPlane(OdGeVector3d axis, OdTvResult rc)
    {
        return _vmAdapter.AddCuttingPlane(axis, rc);
    }

    public void RemoveCuttingPlanes()
    {
        _vmAdapter.RemoveCuttingPlanes();
    }

    public void ShowSectioningOptions()
    {
        _vmAdapter.ShowSectioningOptions();
    }

    public OdGeVector3d GetEyeDirection()
    {
        return _vmAdapter.GetEyeDirection();
    }

    public bool ShowCuttingPlanes()
    {
        return _vmAdapter.ShowCuttingPlanes();
    }
    private class FileProgressBar : IDisposable
    {
        private readonly HclCadImageViewModel _adapter;
        private readonly OverlayViewModel _overlay;
        public FileProgressBar(HclCadImageViewModel adapter, OverlayViewModel overlay, string filePath)
        {
            _adapter=adapter;
            _overlay = overlay;
            _overlay.IsLoading = true;
            _overlay.Title = $"'{Path.GetFileName(filePath)}' is loading..";
        }

        public void OnRegisterCancel(Action action)
        {
            _overlay.CancelTokenSource = new CancellationTokenSource();
            var cancellationToken = _overlay.CancelTokenSource.Token;
            cancellationToken.Register(() =>
            {
                _adapter.OnFileLoadingCancelled();
                action.Invoke();
            });
        }

        public void Dispose()
        {
            _adapter.Refresh();
            _overlay.IsLoading = false;
        }
    }
    public async void LoadFile(string filepath)
    {
        using var progress = new FileProgressBar(_vmAdapter, _overlayViewModel, filepath);
        progress.OnRegisterCancel(() =>
        {
            MainWindowVm.ClearRenderArea();
            SetFileLoaded(false, filepath, (statusText) => _serviceFactory.EventSrv.GetEvent<AppStatusTextChanged>().Publish(statusText), true);
        });
        await _vmAdapter.LoadFile(filepath, _overlayViewModel.Token);
    }

    public void ShowCustomModels()
    {
        _vmAdapter.ShowFps();
        _vmAdapter.ShowWcs();
        //_vmAdapter.ShowCube();
    }

    public void UpdateView()
    {
        _vmAdapter.UpdateCadView();
    }
    public void ShowTool(HclToolType type)
    {
        _vmAdapter.ShowTool(type);
    }
    public void ApplyFillColor()
    {
        _vmAdapter.ApplyFillColor();
    }
    public void ShowLayers()
    {
        var cadLayersView = new CadLayersView();
        var cadLayersViewModel = new CadLayersViewModel(_serviceFactory);
        cadLayersView.DataContext = cadLayersViewModel;
        Window window = new Window
        {
            Title = "CAD Layers Panel",
            Content = cadLayersView,
            SizeToContent = SizeToContent.WidthAndHeight,
            ResizeMode = ResizeMode.NoResize,
            WindowStartupLocation = WindowStartupLocation.CenterScreen
        };
        window.Show();
    }
}

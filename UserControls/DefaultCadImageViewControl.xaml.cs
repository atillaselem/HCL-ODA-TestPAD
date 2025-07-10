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
using System.Windows.Controls.Primitives;
using HCL_ODA_TestPAD.HCL.UserActions;
using HCL_ODA_TestPAD.Utility;
using static HCL_ODA_TestPAD.HCL.CadSelectionConstants;

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
    private readonly UserSelectionWindow _userSelectionWindow;
    private bool _isTouchUpSelection;

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
        _userSelectionWindow = new UserSelectionWindow(
            () => dragSelectionWindow,
            () => dragSelectionWindowBorder);
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
        //If ZoomToArea selected and SelectionMode is Window, then use the userSelectionWindow to handle the mouse event
        if (e.MiddleButton == MouseButtonState.Pressed 
            && _userSelectionWindow.IsEnabled)
        {
            _userSelectionWindow.HandleMouseTouchDown(e, this);
            return;
        }
        if (_userSelectionWindow.IsEnabled)
        {
            // If the userSelectionWindow is enabled, then do not allow any other mouse action.
            return;
        }

        _userActionState.ExecuteMouseTouchDown(e, this); 
        if (MainWindowVm.AppMainWindow.HclToolStationBtn.IsChecked != null && MainWindowVm.AppMainWindow.HclToolStationBtn.IsChecked.Value && (e.MiddleButton == MouseButtonState.Pressed))
        {
            _vmAdapter.ChangeStationLocation(e.GetPosition(this));
        }
        else if (MainWindowVm.AppMainWindow.HclToolPrismBtn.IsChecked != null && MainWindowVm.AppMainWindow.HclToolPrismBtn.IsChecked.Value && (e.RightButton == MouseButtonState.Pressed))
        {
            _vmAdapter.ChangePrismLocation(e.GetPosition(this));
        }
        else if (_userActionState.ActiveAction is UserInteraction.Orbiting or UserInteraction.ZoomToScale && _serviceFactory.AppSettings.HidePointTextTransformation)
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

        //Drag only for creating Selection Window and return;
        if (e.MiddleButton == MouseButtonState.Pressed 
            && _userSelectionWindow.IsEnabled)
        {
            _userSelectionWindow.HandleMouseTouchMove(e, this);
            return;
        }

        if (e.LeftButton == MouseButtonState.Pressed && _userActionState.ActiveAction == UserInteraction.ZoomToArea)
        {
            using var _ = _vmAdapter.UpdateStatusBarCoordinates(e.GetPosition(this));
        }
        //Run Auto Select while mouse is hovering on any entity.
        if (e.LeftButton != MouseButtonState.Pressed && _vmAdapter.AutoSelectWhileHover)
        {
            _vmAdapter.DoSelection(e.GetPosition(this));
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
        else if (e.ChangedButton == MouseButton.Middle && e.MiddleButton == MouseButtonState.Released
            && _userSelectionWindow.IsEnabled)
        {
            _userSelectionWindow.HandleMouseTouchUp(e, this);
            DoMouseTouchUpSelection();
            return;
        }

        if (_isTouchUpSelection)
        {
            _isTouchUpSelection = false;
            // Do not do selection below unless mouse is used.
            return;
        }
        if (_userActionState.ActiveAction is not UserInteraction.ZoomToArea && e.ChangedButton == MouseButton.Left)
        {
            //Selection by Mouse Release is by default.
            _vmAdapter.DoSelection( e.GetPosition(this));
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
        if (_userSelectionWindow.IsEnabled)
        {
            _userSelectionWindow.HandleMouseTouchDown(e, this);
        }
    }
    protected override void OnTouchMove(TouchEventArgs e)
    {
        _actionZoomToScale.ExecuteMouseTouchMove(e, this);
        _vmAdapter.UpdateHclZoomTransformations(isPinchZoom: true);
        //Drag only for creating Selection Window and return;
        if (_userSelectionWindow.IsEnabled)
        {
            _userSelectionWindow.HandleMouseTouchMove(e, this);
        }
    }
    protected override void OnTouchUp(TouchEventArgs e)
    {
        _actionZoomToScale.ExecuteMouseTouchUp(e, this);
        if (_userSelectionWindow.IsEnabled)
        {
            _userSelectionWindow.HandleMouseTouchUp(e, this);
            DoMouseTouchUpSelection();
            _isTouchUpSelection = true;
        }
    }

    private void DoMouseTouchUpSelection()
    {
        _vmAdapter.DoSelection(_userSelectionWindow.SelectionWindowScreenPoints.ToArray());
        _userSelectionWindow.ClearSelectionPoints();
        ChangeSelectionModeToPoint();
    }

    private void ChangeSelectionModeToPoint()
    {
        // 1. Disable the userSelectionWindow after selection is done.
        _userSelectionWindow.IsEnabled = false;
        // 2. Reset the selection mode to Point to enable other user interactions like Panning, Orbiting etc
        MainWindowVm.AppMainWindow.CadSelectionBtnMenuItemPoint.IsChecked = true;
        _vmAdapter.SelectionMode = SM_Point;
        MainWindowVm.AppMainWindow.CadSelectionBtnMenuItemWindow.IsChecked = false;
        MainWindowVm.AppMainWindow.CadSelectionBtnMenuItemCrossing.IsChecked = false;
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

    public void ToggleSelectionWindow()
    {
        _userSelectionWindow.IsEnabled = !_userSelectionWindow.IsEnabled;
    }

    public void OnClickSelectionMenu(string menuSelected)
    {
        var stackPanel = MainWindowVm.AppMainWindow.CADSelectionPanel;
        var btnPrefix = "CadSelectionBtnMenuItem";
        switch (menuSelected)
        {
            case SelectionEnable:
            {
                _vmAdapter.SelectionEnabled = !_vmAdapter.SelectionEnabled;
                break;
            }
            case SL_Entities:
            {
                if (IsPressedOnSameSelectionLevel(SL_Entities))
                {
                    break;
                }
                if (WpfHelper.FindChild(stackPanel, child =>
                        child is ToggleButton
                        {
                            Tag: SL_Entities,
                            IsChecked: true
                        }) is ToggleButton windowMenu)
                {
                    CheckOnMenuItems(false, SL_Geometries);
                }
                break;
            }
            case SL_Geometries:
            {
                if (IsPressedOnSameSelectionLevel(SL_Geometries))
                {
                    break;
                }
                if (WpfHelper.FindChild(stackPanel, child =>
                        child is ToggleButton
                        {
                            Tag: SL_Geometries,
                            IsChecked: true
                        }) is ToggleButton windowMenu)
                {
                    CheckOnMenuItems(false, SL_Entities);
                }
                break;
            }
            case AutoSelection:
            {
                _vmAdapter.AutoSelectWhileHover = !_vmAdapter.AutoSelectWhileHover;
                break;
            }   
            case SM_Point:
            {
                if (IsPressedOnSameSelectionMode(SM_Point))
                {
                    break;
                }
                if (WpfHelper.FindChild(stackPanel, child =>
                child is ToggleButton
                {
                    Tag: SM_Point,
                    IsChecked: true
                }) is ToggleButton windowMenu)
                {
                    ToggleAutoSelection(false, true);
                    CheckOnMenuItems(false, SM_Window, SM_Crossing);
                    _userSelectionWindow.IsEnabled = false;
                }
                break;
            }
            case SM_Window:
            {
                if (IsPressedOnSameSelectionMode(SM_Window))
                {
                    break;
                }
                if (WpfHelper.FindChild(stackPanel, child =>
                        child is ToggleButton
                        {
                            Tag: SM_Window,
                            IsChecked:true
                        }) is ToggleButton windowMenu)
                {
                    ToggleAutoSelection(false, false);
                    CheckOnMenuItems(false, SM_Point, SM_Crossing);
                    _userSelectionWindow.IsEnabled = true;
                }
                break;
            }   
            case SM_Crossing:
            {
                if (IsPressedOnSameSelectionMode(SM_Crossing))
                {
                    break;
                }
                if (WpfHelper.FindChild(stackPanel, child =>
                child is ToggleButton
                {
                    Tag: SM_Crossing,
                    IsChecked: true
                }) is ToggleButton crossingMenu)
                {
                    ToggleAutoSelection(false, false);
                    CheckOnMenuItems(false,  SM_Point, SM_Window);
                    _userSelectionWindow.IsEnabled = true;
                }
                break;
            }
        }

        return;

        #region Local Selection Menu Functions
        bool IsPressedOnSameSelectionLevel(string selectionLevel)
        {
            if (_vmAdapter.SelectionLevel == selectionLevel)
            {
                CheckOnMenuItems(true, selectionLevel);
                return true;
            }

            _vmAdapter.SelectionLevel = selectionLevel;
            _vmAdapter.SetSelectionLevel();
            return false;
        }
        bool IsPressedOnSameSelectionMode(string selectionMode)
        {
            if (_vmAdapter.SelectionMode == selectionMode)
            {
                CheckOnMenuItems(true, selectionMode);
                return true;
            }

            _vmAdapter.SelectionMode = selectionMode;
            return false;
        }
        void ToggleAutoSelection(bool isChecked, bool isEnabled)
        {
            var autoSelectMenu = stackPanel.FindName(btnPrefix + AutoSelection.Replace(" ", "")) as ToggleButton;
            autoSelectMenu.IsChecked = isChecked;
            autoSelectMenu.IsEnabled = isEnabled;
            _vmAdapter.AutoSelectWhileHover = isChecked;
        }
        void CheckOnMenuItems(bool isChecked, params string[] menuItems)
        {
            foreach (var menuItemName in menuItems)
            {
                var menuItem = stackPanel.FindName(btnPrefix + menuItemName.Replace(" ", "")) as ToggleButton;
                menuItem.IsChecked = isChecked;
            }
        }
        #endregion

    }
}

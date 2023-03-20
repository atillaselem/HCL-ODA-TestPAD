using HCL_ODA_TestPAD.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Teigha.Visualize;
using HCL_ODA_TestPAD.Services;
using HCL_ODA_TestPAD.Settings;
using Prism.Events;
using HCL_ODA_TestPAD.ViewModels.Base;
using HCL_ODA_TestPAD.ODA.Draggers;
using HCL_ODA_TestPAD.ODA.ModelBrowser;
using Teigha.Core;
using System.Collections.Generic;
using System.ComponentModel;
using HCL_ODA_TestPAD.HCL.MouseTouch;
using System.Diagnostics.CodeAnalysis;

namespace HCL_ODA_TestPAD.UserControls;

/// <summary>
/// Interaction logic for CadImageViewControl.xaml
/// </summary>
public partial class DefaultCadImageViewControl : IOpenGLES2Control, ICadImageViewControl
{
    private readonly IServiceFactory _serviceFactory;

    private readonly HclCadImageViewModel _vmAdapter;
    //public HclCadImageViewModel Adapter => _vmAdapter;
    public MainWindowViewModel VM { get; set; }
    private ZoomToAreaManager _zoomToAreaManager;
    private ZoomToScaleManager _zoomToScaleManager;
    public string FilePath => throw new NotImplementedException();

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
    
    private readonly List<int> _arrTouches = new();
    private bool _isPinchZooming;

    public DefaultCadImageViewControl(MainWindowViewModel vm, IServiceFactory serviceFactory)
    {
        InitializeComponent();
        VM = vm;
        _serviceFactory = serviceFactory;
        _vmAdapter = new HclCadImageViewModel(serviceFactory);
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

    public void SetFileLoaded(bool isFileLoaded, string filePath, Action<string> emitEvent)
    {
        VM.FileIsExist = isFileLoaded;
        if (isFileLoaded)
        {
            VM.PanCommand_Clicked();
            emitEvent?.Invoke($"File : [{filePath}] loaded successfully.");
            _vmAdapter.ShowCustomModels();
            if (_zoomToScaleManager is null)
            {
                _zoomToScaleManager = new ZoomToScaleManager(this, _vmAdapter.TvGsDeviceId, _serviceFactory.AppSettings);
            }
        }
        else
        {
            emitEvent?.Invoke($"File : [{filePath}] unloaded.");
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
    #region Overriden Mouse Events
    protected override void OnMouseDown(MouseButtonEventArgs e)
    {

        if (_zoomToAreaManager is not null && _zoomToAreaManager.IsZoomToAreaEnabled)
        {
            _zoomToAreaManager.HandleTouchAndMouseDown(e, this);
        }
        else
        {
            if (!_isPinchZooming)
            {
                _vmAdapter.MouseDown(e, e.GetPosition(this));
            }
        }
    }

    protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
    {
        if (_zoomToAreaManager is not null && _zoomToAreaManager.IsZoomToAreaEnabled)
        {
            _zoomToAreaManager.HandleTouchAndMouseMove(e, this);
        }
        else
        {
            if (!_isPinchZooming)
            {
                _vmAdapter.MouseMove(e, e.GetPosition(this));
            }
        }
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        if (_zoomToAreaManager is not null && _zoomToAreaManager.IsZoomToAreaEnabled)
        {
            _zoomToAreaManager.HandleTouchAndMouseUp(e, this);
        }
        else
        {
            if (!_isPinchZooming)
            {
                _vmAdapter.MouseUp(e);
            }
        }
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        _vmAdapter.MouseWheel(e);
    }
    #endregion

    #region Overriden Touch Events
    protected override void OnTouchDown(TouchEventArgs e)
    {
        if (_zoomToAreaManager.IsZoomToAreaEnabled)
        {
            _zoomToAreaManager.HandleTouchAndMouseDown(e, this);
        }
        else
        {
            _isPinchZooming = _zoomToScaleManager.HandleTouchAndMouseDown(e, this);
        }
    }
    protected override void OnTouchMove(TouchEventArgs e)
    {
        if (_zoomToAreaManager is not null && _zoomToAreaManager.IsZoomToAreaEnabled)
        {
            _zoomToAreaManager.HandleTouchAndMouseMove(e, this);
        }
        else
        {
            _zoomToScaleManager.HandleTouchAndMouseMove(e, this);
        }
    }
    protected override void OnTouchUp(TouchEventArgs e)
    {
        if (_zoomToAreaManager is not null && _zoomToAreaManager.IsZoomToAreaEnabled)
        {
            _zoomToAreaManager.HandleTouchAndMouseUp(e, this);
        }
        else
        {
            _zoomToScaleManager.HandleTouchAndMouseUp(e, this);
            _isPinchZooming = false;
        }
    }
    #endregion
    public void Pan()
    {
        _vmAdapter.Pan();
    }

    public void Orbit()
    {
        _vmAdapter.Orbit();
    }

    public void ZoomToArea(bool enable)
    {
        if (_zoomToAreaManager is null)
        {
            _zoomToAreaManager = new ZoomToAreaManager(this, () => dragSelectionCanvas, () => dragSelectionBorder, _vmAdapter.TvGsDeviceId);
        }

        _zoomToAreaManager.IsZoomToAreaEnabled = enable;
    }

    public void SetRenderModeButton(OdTvGsView.RenderMode mode)
    {
        VM.SetRenderModeButton(mode);
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

    public void Regen(OdTvGsDevice.RegenMode rm)
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

    public void Set3DView(OdTvExtendedView.e3DViewType type)
    {
        _vmAdapter.Set3DView(type);
    }

    public void SetProjectionType(OdTvGsView.Projection projection)
    {
        _vmAdapter.SetProjectionType(projection);
    }

    public void SetRenderMode(OdTvGsView.RenderMode renderMode)
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

    public void LoadFile(string filepath)
    {
        _vmAdapter.LoadFile(filepath);
    }

    public void ShowCustomModels()
    {
        _vmAdapter.ShowFPS();
        _vmAdapter.ShowWCS();
        //_vmAdapter.ShowCube();
    }

    public void UpdateView()
    {
        _vmAdapter.UpdateCadView();
    }


}

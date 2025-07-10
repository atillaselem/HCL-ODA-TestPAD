using System.Threading.Tasks;
using HCL_ODA_TestPAD.Mvvm.Events;
using HCL_ODA_TestPAD.HCL;
using HCL_ODA_TestPAD.Settings;
using System.Windows;
using HCL_ODA_TestPAD.ViewModels.Base;
using HCL_ODA_TestPAD.HCL.Tools;
using ODA.Kernel.TD_RootIntegrated;
using ODA.Visualize.TV_Visualize;
using System.Diagnostics;
using System;
using HCL_ODA_TestPAD.ODA.WCS;
using ODA.Visualize.TV_VisualizeTools;

namespace HCL_ODA_TestPAD.ViewModels;

// ALC : Application Life Cycle
public partial class HclCadImageViewModel
{
    #region HCL Domain Variables
    private CadModel _cadModel;
    private CadImageViewModel _cadImageViewModel;

    #endregion
    #region ODA - Visualize Database, Device, Model variables
    private OdTvModel TvTopModel;
    private OdTvModelId TvTopModelId;
    public int TvActiveViewport = -1;
    private OdTvDatabaseId _dbId = null;
    public OdTvDatabaseId TvDatabaseId
    {
        get { return _dbId; }
        set { _dbId = value; }
    }
    public OdTvGsDeviceId TvGsDeviceId { get; set; }
    public OdTvModelId TvDefaultModelId { get; set; }
    #endregion

    public IServiceFactory ServiceFactory { get; }
    public ICadImageViewControl ViewControl { get; set; }
    #region App Specific Variables
    public MainWindowViewModel Vm { get; set; }
    public string FilePath { get; private set; }
    public bool AddDefaultViewOnLoad { get; set; }
    private bool _isInitialized, _isFileLoading, _isIfc;
    #endregion


    public HclCadImageViewModel(IServiceFactory serviceFactory)
        : base(serviceFactory)
    {
        ServiceFactory = serviceFactory;
    }

    public override Task LoadCadModelViewAsync()
    {
        return Task.CompletedTask;
    }
    public void InitViewModel()
    {
        var logger = new TestPadLogger();
        var cadGenFactory = new CadRegenerator(ServiceFactory);
        _cadModel = new CadModel(() => cadGenFactory);
        _cadImageViewModel = new CadImageViewModel(ViewControl, logger, _cadModel, new CadImageViewBitmapService());
        ServiceFactory.EventSrv.GetEvent<PrismTypeChangedEvent>().Subscribe(OnPrismTypeChanged);
    }
    public void RenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        _widthResized = (int)sizeInfo.NewSize.Width;
        _heightResized = (int)sizeInfo.NewSize.Height;

        if ((sizeInfo.WidthChanged || sizeInfo.HeightChanged) && sizeInfo.NewSize.Width > 0 && sizeInfo.NewSize.Height > 0)
        {
            OnRenderSizeChanged(sizeInfo.NewSize);
        }
    }
    public void VisibilityChanged(bool visibility)
    {
        _cadImageViewModel.OnVisibilityChanged(visibility);
        if (visibility)
        {
            AddDefaultView();
        }
        else
        {
            ClearOda();
        }
    }
    public void OnRenderSizeChanged(Size size)
    {
        _cadImageViewModel.OnRenderSizeChanged(size);
    }
    public void Update()
    {
        _cadImageViewModel.Update();
    }

    public void UpdateCadView(bool invalidate = false)
    {
        _cadModel.Update(invalidate);
    }
    public void ClearOda()
    {
        ClearDevices();
        ClearDatabases();
        ViewControl?.SetFileLoaded(false, FilePath, (statusText) => ServiceFactory.EventSrv.GetEvent<AppStatusTextChanged>().Publish(statusText));
        _isFileLoading = false;
        _isIfc = false;
        ServiceFactory.EventSrv.GetEvent<PrismTypeChangedEvent>().Unsubscribe(OnPrismTypeChanged);
    }
    public void OnFileLoadingCancelled()
    {
        ClearDevices();
        ClearDatabases();
    }
    private void ClearDevices()
    {
        try
        {
            _cadImageViewModel.OnVisibilityChanged(false);

            MemoryTransaction mtr = _mm.StartTransaction();

            SelectionSet?.Dispose();
            SelectionSet = null;

            TvDefaultModelId?.Dispose();
            TvDefaultModelId = null;

            TvTopModelId?.Dispose();
            TvTopModelId = null;

            HclStationLocation?.Dispose();
            HclStationLocation = null;

            HclPrismLocation?.Dispose();
            HclPrismLocation = null;

            HclStationBuilder.Dispose(this);
            _hclStation = null;
            HclPrismBuilder.Dispose(this);
            _hclPrism = null;
            HclToolLineBuilder.Dispose(this);
            _hclToolLine = null;
            HclPointBuilder.Dispose(this);
            _hclPointContainer?.Dispose();
            _hclPointContainer = null;

            _cachedViewCenter?.Dispose();
            _cachedViewCenter = null;

            if (TvGsDeviceId != null)
            {
                foreach (var extendedView in _extendedViewDict.Values)
                {
                    extendedView.Dispose();
                }
                _extendedViewDict.Clear();
                OdTvGsDevice dev = TvGsDeviceId.openObject(OdTv_OpenMode.kForWrite);
                if (dev != null)
                {
                    for (int i = 0; i < dev.numViews(); i++)
                    {
                        dev.viewAt(i).openObject().Dispose();
                    }
                }
                if (TvDatabaseId != null && !TvDatabaseId.isNull() && TvDatabaseId.isValid())
                    TvDatabaseId
                        .openObject(OdTv_OpenMode.kForWrite)
                        .clearDevices();

                TvGsDeviceId.Dispose();
                TvGsDeviceId = null;
            }
            _mm.StopTransaction(mtr);

            _cadModel.Dispose();
            Debug.WriteLine($"ClearDevices : {FilePath}");
        }
        catch (Exception)
        {
            Debug.WriteLine($"ClearDatabases : Exception caught during FileLoad Cancellation");
            throw;
        }
    }
    private void ClearDatabases()
    {
        try
        {
            OdTvFactoryId factId = TV_Visualize_Globals.odTvGetFactory();
            factId.clearDatabases();
        }
        catch (Exception)
        {
            Debug.WriteLine($"ClearDatabases : Exception caught during FileLoad Cancellation");
        }

    }
    public override void CloseTabView()
    {
        ClearOda();
    }
    public void ShowFps()
    {
        OnOffFPS(ServiceFactory.AppSettings.ShowFps);
    }
    public void ShowWcs()
    {
        OnOffWCS(ServiceFactory.AppSettings.ShowWcs);
    }
    public void ShowCube()
    {
        OnOffViewCube(ServiceFactory.AppSettings.ShowCube);
    }
    public void ShowCustomModels()
    {
        ShowFps();
        ShowWcs();
    }
    internal void Refresh()
    {
        _cadImageViewModel.OnRenderSizeChanged(new Size(_widthResized, _heightResized));
    }
    public void OnOffWCS(bool bEnable)
    {
        if (TvGsDeviceId == null)
            return;

        if (bEnable)
        {
            if (Wcs == null)
                CreateWcs();
            else
                Wcs.UpdateWcs();
        }
        else if (!bEnable && Wcs != null)
        {
            Wcs.RemoveWcs();
        }
        //MemoryTransaction mtr = MM.StartTransaction();
        //TvDeviceId.openObject().update();
        //Invalidate();
        UpdateCadView();
        //MM.StopTransaction(mtr);
    }

    private void CreateWcs()
    {
        if (Wcs != null)
            Wcs.RemoveWcs();

        MemoryTransaction mtr = _mm.StartTransaction();
        using var odTvGsDevice = TvGsDeviceId.openObject();
        Wcs = new TvWpfViewWcs(TvDatabaseId, odTvGsDevice.viewAt(TvActiveViewport));
        OdTvGsView pWcsView = Wcs.GetWcsViewId().openObject(OdTv_OpenMode.kForWrite);
        OdTvGsView activeView = Wcs.GetParentViewId().openObject();
        OdGePoint3d activeViewPos = activeView.position();
        //Identity Matrix
        OdGeMatrix3d wcsMatrix = new OdGeMatrix3d();
        //World matrix translated to active Camera's target position.
        wcsMatrix.setTranslation(-activeView.target().asVector());
        //Create a camera view for WCS
        pWcsView.setView(activeViewPos.transformBy(wcsMatrix), OdGePoint3d.kOrigin, activeView.upVector(), 1, 1);
        //pWcsView.setMode(activeView.mode());
        pWcsView.setMode(OdTvGsView_RenderMode.kGouraudShaded);
        pWcsView.zoom(4.2);
        OdGePoint2d lowerLeft = new OdGePoint2d();
        OdGePoint2d upperRight = new OdGePoint2d();
        activeView.getViewport(lowerLeft, upperRight);
        upperRight.x = lowerLeft.x + 1.9;
        upperRight.y = lowerLeft.y + 0.23;
        pWcsView.setViewport(lowerLeft, upperRight);
        Wcs.UpdateWcs();

        _mm.StopTransaction(mtr);
    }

    public void OnOffViewCube(bool bEnable)
    {
        if (TvGsDeviceId == null)
            return;
        MemoryTransaction mtr = _mm.StartTransaction();
        OdTvExtendedView extView = GetActiveTvExtendedView();
        if (extView != null && extView.getEnabledViewCube() != bEnable)
        {
            extView.setEnabledViewCube(bEnable);
            //Invalidate();
            UpdateCadView();
        }
        _mm.StopTransaction(mtr);
    }

    public void OnOffFPS(bool bEnable)
    {
        if (TvGsDeviceId == null)
            return;
        //MemoryTransaction mtr = MM.StartTransaction();
        using var dev = TvGsDeviceId.openObject(OdTv_OpenMode.kForWrite);
        if (dev.getShowFPS() != bEnable)
        {
            dev.setShowFPS(bEnable);
            //dev.update();
            //Invalidate();
            UpdateCadView();
        }
        //MM.StopTransaction(mtr);
    }

    public void OnOffAnimation(bool bEnable)
    {
        if (TvGsDeviceId == null)
            return;
        MemoryTransaction mtr = _mm.StartTransaction();
        OdTvExtendedView exView = GetActiveTvExtendedView();
        if (exView != null)
            exView.setAnimationEnabled(bEnable);
        _mm.StopTransaction(mtr);
    }

    public void SetZoomStep(double dValue)
    {
        if (TvGsDeviceId == null || dValue < 1)
            return;
        OdTvExtendedView exView = GetActiveTvExtendedView();
        if (exView != null)
            exView.setZoomScale(dValue);
    }
}
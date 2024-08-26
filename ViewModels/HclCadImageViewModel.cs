using HCL_ODA_TestPAD.Dialogs;
using HCL_ODA_TestPAD.Functional.Extensions;
using HCL_ODA_TestPAD.HCL;
using HCL_ODA_TestPAD.Mvvm.Events;
using HCL_ODA_TestPAD.ODA.Draggers.Construct;
using HCL_ODA_TestPAD.ODA.Draggers.Markups;
using HCL_ODA_TestPAD.ODA.Draggers.Navigation;
using HCL_ODA_TestPAD.ODA.Draggers;
using HCL_ODA_TestPAD.ODA.ModelBrowser;
using HCL_ODA_TestPAD.ODA.WCS;
using HCL_ODA_TestPAD.Performance;
using HCL_ODA_TestPAD.Settings;
using HCL_ODA_TestPAD.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using HCL_ODA_TestPAD.HCL.CadUnits;
using MessageBox = System.Windows.Forms.MessageBox;
using System.Windows.Forms;
using ODA.Kernel.TD_RootIntegrated;
using ODA.Visualize.TV_Visualize;
using ODA.Visualize.TV_VisualizeTools;
using HCL_ODA_TestPAD.HCL.Visualize;
using HCL_ODA_TestPAD.HCL.CAD.Math.API;
using HCL_ODA_TestPAD.HCL.Visualize.Extensions;
using System.Threading;
using System.Runtime.ExceptionServices;
using System.Security;
using HCL_ODA_TestPAD.HCL.UserActions.States;
using HCL_ODA_TestPAD.HCL.Profiler;
using Microsoft.Extensions.Logging;

namespace HCL_ODA_TestPAD.ViewModels;
public enum VisibleEntityType
{
    PrismImageEntity,
    ArrowLineEntity,
    ArrowEntity,
    StationImageEntity
}

public class HclCadImageViewModel : CadImageTabViewModelBase,
    IOdaSectioning, IHclTooling
{
    public IServiceFactory ServiceFactory { get; }
    public ICadImageViewControl ViewControl { get; set; }

    #region App Specific Variables
    public MainWindowViewModel Vm { get; set; }
    public string FilePath { get; private set; }
    //public double Width { get; set; }
    //public double Height { get; set; }
    public bool AddDefaultViewOnLoad { get; set; }
    #endregion

    #region Image Bufferring Variables
    bool _isInitialized, _isFileLoading;
    static int _widthResized, _heightResized;
    private CadImageViewModel _cadImageViewModel;
    #endregion

    #region Unit Variables
    private UnitsValue CadUnits { get; set; }
    #endregion

    #region ODA Visualize Variables
    private readonly MemoryManager _mm = MemoryManager.GetMemoryManager();
    
    // Visualize database
    private OdTvDatabaseId _dbId = null;
    public ulong InnerRectangleEntityHandle { get; internal set; }
    public OdTvDatabaseId TvDatabaseId
    {
        get { return _dbId; }
        set { _dbId = value; }
    }
    public OdTvGsDeviceId TvGsDeviceId { get; set; }
    public OdTvModelId TvDefaultModelId { get; set; }
    public OdTvGeometryDataId TvPolygonGeometryId { get; set; }
    // current active dragger
    private ODA.Draggers.OdTvDragger _dragger = null;
    // index of active view
    public int TvActiveViewport = -1;
    private OdTvModelId _tvActiveModelId = null;
    private OdTvModelId _tvMarkupModelId = null;
    private OdTvModelId _tvDraggersModelId = null;
    // cutting planes
    private OdTvModelId _cuttingPlanesModelId = null;
    public OdTvModelId CuttingPlaneModelId { get { return _cuttingPlanesModelId; } }
    private OdTvGsViewId _cuttingPlanesViewId = null;
    public OdTvGsViewId CuttingPlanesViewId { get { return _cuttingPlanesViewId; } }
    public OdTvSectioningOptions SectioningOptions { get; private set; }
    public static int _odTvCuttingPlaneMaxNum = 5;

    public static OdTvRegAppId AppTvId { get; set; }
    private Dictionary<ulong, OdTvExtendedView> _extendedViewDict = new Dictionary<ulong, OdTvExtendedView>();

    private OdTvAnimation _animation = null;

    // list with selected items(bold nodes in model browser)
    public List<TvTreeItem> SelectedNodes = new List<TvTreeItem>();

    public OdTvSelectionSet SelectionSet = null;
    public TvWpfViewWcs Wcs { get; set; }
    public TvDatabaseInfo DatabaseInfo { get; set; }
    private CadModel _cadModel;
    #endregion

    #region HclTools Variables
    private CadPoint3D _cachedViewCenter;
    public CadPoint3D HclPrismLocation { get; set; }
    public CadPoint3D HclStationLocation { get; set; }
    public double LastRadius { get; set; } = 1;
    public double PointScaleFactor { get; set; } = 1;
    public double PrismScaleFactor { get; set; } = 1;
    private HclStation _hclStation;
    private HclPrism _hclPrism;

    private HclPointContainer _hclPointContainer;
    private CadLine _hclToolLine;
    public bool PointScaleFactorSet { get; set; }
    public bool PrismScaleFactorSet { get; set; }
    private int _pointMoveCycle = default;
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
    private void OnPrismTypeChanged(string prismType)
    {
        HclPrismBuilder.Dispose(this);
        ShowTool(HclToolType.Prism);
        //_hclPrism?.UpdateViewTransformation();
        //UpdateCadView();
    }

    private void AddDefaultView()
    {
        if (!AddDefaultViewOnLoad) return;

        if (!_isInitialized && !_isFileLoading)
        {
            var profiler = Profiler.Start("GenerateDefaultCadImage");
            GenerateDefaultCadImage((int)_widthResized, (int)_heightResized);
            profiler.Stop();
            _isInitialized = true;
        }
    }
    public void RenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        _widthResized = (int)sizeInfo.NewSize.Width;
        _heightResized = (int)sizeInfo.NewSize.Height;

        if ((sizeInfo.WidthChanged || sizeInfo.HeightChanged) && sizeInfo.NewSize.Width > 0 && sizeInfo.NewSize.Height > 0)
        {
            OnRenderSizeChanged(sizeInfo.NewSize);
        }

        //UpdateCadView(true);
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
    #region Create GL_Device
    public void GenerateDefaultCadImage(int width, int height)
    {
        using var odTvFactoryId = TV_Visualize_Globals.odTvGetFactory();
        TvDatabaseId = odTvFactoryId.createDatabase();
        try
        {
            OdTvResult rc = OdTvResult.tvCannotOpenFile;
            using var odTvDatabase = TvDatabaseId.openObject(OdTv_OpenMode.kForWrite, ref rc);
            // Create model
            var odTvModelId = odTvDatabase.createModel("Tv_Model_Default", OdTvModel_Type.kMain);
            TvDefaultModelId = odTvModelId;
            // Create entity
            using var odModelObj = odTvModelId.openObject(OdTv_OpenMode.kForWrite);
            using var enId = odModelObj.appendEntity("Entity_Default");
            {
                // Create and setup text style in database
                using var textStyle = odTvDatabase.createTextStyle("kMiddleCenter");
                {
                    using var pTextStyle = textStyle.openObject(OdTv_OpenMode.kForWrite);

                    var typeface = "Algerian";
                    var charset = 0;
                    var family = 34;
                    var bold = true;
                    var italic = true;
                    pTextStyle.setFont(typeface, bold, italic, charset, family);
                    pTextStyle.setAlignmentMode(OdTvTextStyle_AlignmentType.kMiddleCenter);
                    pTextStyle.setTextSize(0.1);
                }

                using var textStyleDef = new OdTvTextStyleDef();
                textStyleDef.setTextStyle(textStyle);

                using var pEn = enId.openObject(OdTv_OpenMode.kForWrite);
                // Create text geometry data
                using var textId1 = pEn.appendText(new OdGePoint3d(-0.010, 0.15, 0.0), "HILTI");
                {
                    // Set color for geometry data
                    using var textIdObj = textId1.openObject();
                    textIdObj.setColor(new OdTvColorDef(204, 0, 51));
                    using var textObject = textId1.openAsText();
                    // Set text style for text
                    textObject.setTextStyle(textStyleDef);
                }
                using var colorDef = new OdTvColorDef(51, 0, 204);
                using var textId2 = pEn.appendText(new OdGePoint3d(0.00, -0.05, 0.0), "HCL");
                {
                    textId2.openObject().setColor(colorDef);
                    using var textObject = textId2.openAsText();
                    textObject.setTextStyle(textStyleDef);
                }

                using var textId3 = pEn.appendText(new OdGePoint3d(-0.025, -0.25, 0.0), "GL_Control");
                {
                    textId3.openObject().setColor(colorDef);
                    using var textObject = textId3.openAsText();
                    textObject.setTextStyle(textStyleDef);
                }

                var pointArray = new OdGePoint3d[4];
                double xMin = -0.5, xMax = 0.5, yMin = -0.5, yMax = 0.5;
                pointArray[0] = new OdGePoint3d(xMin, yMin, 0.0);
                pointArray[1] = new OdGePoint3d(xMin, yMax, 0.0);
                pointArray[2] = new OdGePoint3d(xMax, yMax, 0.0);
                pointArray[3] = new OdGePoint3d(xMax, yMin, 0.0);

                // Create polygon geometry data
                using var rectId = pEn.appendPolygon(pointArray);
                {
                    using var pGeom = rectId.openObject();
                    pGeom.setColor(new OdTvColorDef(128, 0, 255));
                    pGeom.setLineWeight(new OdTvLineWeightDef(7));
                }
                pointArray.ForEach(p => p.Dispose());
                
            }
            using var innerRectangleEntityId = odModelObj.appendEntity("Entity_Arrow");
            {
                using var innerRectangleEntityObj = innerRectangleEntityId.openObject(OdTv_OpenMode.kForWrite);

                var pointArray = new OdGePoint3d[4];
                double xMin = -0.48, xMax = 0.48, yMin = -0.48, yMax = 0.48;
                pointArray[0] = new OdGePoint3d(xMin, yMin, 0.0);
                pointArray[1] = new OdGePoint3d(xMin, yMax, 0.0);
                pointArray[2] = new OdGePoint3d(xMax, yMax, 0.0);
                pointArray[3] = new OdGePoint3d(xMax, yMin, 0.0);

                // Create polygon geometry data
                using var polygonId = innerRectangleEntityObj.appendPolygon(pointArray);
                {
                    using var pGeom = polygonId.openObject();
                    pGeom.setColor(new OdTvColorDef(128, 0, 128));
                    pGeom.setLineWeight(new OdTvLineWeightDef(7));
                }
                TvPolygonGeometryId = polygonId;
                pointArray.ForEach(p => p.Dispose());
                InnerRectangleEntityHandle = innerRectangleEntityObj.getDatabaseHandle();
            }

            ServiceFactory.AppSettings = ServiceFactory.AppSettings with { CadFileUnit = SurveyUnits.Meters };
            CreateDefaultBitmapDevice(width, height, odTvDatabase, odTvModelId);
           
            ViewControl?.SetFileLoaded(true, FilePath, (statusText) => ServiceFactory.EventSrv.GetEvent<AppStatusTextChanged>().Publish(statusText));
        }
        catch
        {
            MessageBox.Show("Cannot create new file!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public void UpdateCadView(bool invalidate = false)
    {
        _cadModel.Update(invalidate);
    }

    private void CreateDefaultBitmapDevice(int width, int height, OdTvDatabase odTvDatabase, OdTvModelId odTvModelId)
    {
        try
        {
            //1-Create device
            TvGsDeviceId = TvDatabaseId?.openObject().createBitmapDevice("TV_BitmapDevice_Default");
            _cadModel.TvGsDeviceId = TvGsDeviceId;
            //2-Open device
            using var odTvGsDevice = TvGsDeviceId?.openObject(OdTv_OpenMode.kForWrite);

            //3-Create view
            using var odTvGsViewId = odTvGsDevice?.createView("TV_View_Default");

            //4-Add view to device
            odTvGsDevice?.addView(odTvGsViewId);

            //5-Add current model to the view
            using var viewRef = odTvGsViewId?.openObject(OdTv_OpenMode.kForWrite);

            //6-Setup view to make it contr directional with the WCS normal
            //viewRef?.setView(new OdGePoint3d(0, 0, 1), new OdGePoint3d(0, 0, 0), new OdGeVector3d(0, 1, 0), 1, 1);

            //7-Add main model to the view
            viewRef?.addModel(odTvModelId);
            //8-Set current view as active
            viewRef?.setActive(true);

            //9-Set the render mode
            //viewRef?.setMode(OdTvGsView.RenderMode.k2DOptimized);

            //10-Setup Bitmap Gs for OpenGLES2
            odTvGsDevice?.setupGsBitmap(0, 0, OdTvGsDevice_Name.kOpenGLES2);
            odTvGsDevice.setOption(OdTvGsDevice_Options.kUseSceneGraph, ServiceFactory.AppSettings.UseSceneGraph);
            odTvGsDevice.setOption(OdTvGsDevice_Options.kForcePartialUpdate, ServiceFactory.AppSettings.UseForcePartialUpdate);
            odTvGsDevice.setOption(OdTvGsDevice_Options.kBlocksCache, ServiceFactory.AppSettings.UseBlocksCache);

            ConfigureViewSettings(odTvGsViewId);


            using var rect = new OdTvDCRect(0, 800, 600, 0);
            odTvGsDevice.onSize(rect);
            odTvGsDevice.update();

            //TvActiveViewport = 0;
            //SetFrozenLayersVisible(odTvDatabase);
            TvActiveViewport = 0;

            SetFrozenLayersVisible(odTvDatabase);
            
            SetupStyles(odTvDatabase);

            FilePath = "HILTI-HCL-GL_Control";

            EmitFileLoadedEvents();

            _isFileLoading = true;

            _cadModel.TvGsDeviceId = TvGsDeviceId;

            Zoom(ZoomType.ZoomExtents);
        }
        catch (Exception e)
        {
            MessageBox.Show("Cannot create device:" + e);
            throw;
        }
    }
    public async Task LoadFile(string filepath, CancellationToken cancellationToken)
    {
        var taskIsIfc = await Task.Run(() => ImportFile(filepath), cancellationToken);
 
        if (TvDatabaseId != null)
        {
            using var odTvDatabase = TvDatabaseId.openObject(OdTv_OpenMode.kForWrite);
            //File Loading cancelled.
            if (odTvDatabase == null)
            {
                return;
            }
            if (ServiceFactory.AppSettings.EnableImportUnitChange)
            {
                SetCadDbUnitAsCadFileUnit(odTvDatabase);
            }
            else
            {
                GetCadDbUnitAsCadFileUnit(odTvDatabase);
            }

            using var importedDevIt = odTvDatabase.getDevicesIterator();
            using var importedDeviceId = importedDevIt.getDevice();
            using var importedDevice = importedDeviceId.openObject(OdTv_OpenMode.kForWrite);
            using var activeViewId = importedDevice.viewAt(0);
            importedDevice.removeView(activeViewId);

            //1-Create bitmap device
            TvGsDeviceId = odTvDatabase.createBitmapDevice("TV_BitmapDevice_DEVICE");
            using var odTvGsDevice = TvGsDeviceId.openObject(OdTv_OpenMode.kForWrite);
            /*Remove Old View*/
            odTvGsDevice.addView(activeViewId);
            odTvGsDevice.setActive(true);
            //2-Setup Bitmap Gs for OpenGLES2
            odTvGsDevice.setupGsBitmap(0, 0, OdTvGsDevice_Name.kOpenGLES2);
            odTvGsDevice.setForbidImageHighlight(ServiceFactory.AppSettings.SetForbidImageHighlight);
            odTvGsDevice.setOption(OdTvGsDevice_Options.kForcePartialUpdate, ServiceFactory.AppSettings.UseForcePartialUpdate);
            odTvGsDevice.setOption(OdTvGsDevice_Options.kBlocksCache, ServiceFactory.AppSettings.UseBlocksCache);
            odTvGsDevice.setOption(OdTvGsDevice_Options.kForceOffscreenSceneGraph, ServiceFactory.AppSettings.UseForceOffscreenSceneGraph);
            odTvGsDevice.setOption(OdTvGsDevice_Options.kSceneGraphPurgeGrouppedRenders, ServiceFactory.AppSettings.UseSceneGraphPurgeGroupedRenders);
            odTvGsDevice.setOption(OdTvGsDevice_Options.kUseVisualStyles, ServiceFactory.AppSettings.UseVisualStyles);

            ConfigureViewSettings(activeViewId);

            if (!taskIsIfc)
            {
                odTvGsDevice.setOption(OdTvGsDevice_Options.kUseSceneGraph, ServiceFactory.AppSettings.UseSceneGraph);
            }
            else
            {
                odTvGsDevice.setOption(OdTvGsDevice_Options.kUseSceneGraph, ServiceFactory.AppSettings.IfcUseSceneGraph);
            }

            using var rect = new OdTvDCRect(0, 800, 600, 0);
            odTvGsDevice.onSize(rect);
            //odTvGsDevice.update();

            TvActiveViewport = 0;

            SetFrozenLayersVisible(odTvDatabase);
            SetupStyles(odTvDatabase);
            FilePath = filepath;

            EmitFileLoadedEvents();

            _isFileLoading = true;

            _cadModel.TvGsDeviceId = TvGsDeviceId;

            //Zoom(ZoomType.ZoomExtents);
            Set3DView(OdTvExtendedView_e3DViewType.kTop);
            Regen(OdTvGsDevice_RegenMode.kRegenAll);
            UpdateCadView(true);
        }
    }
#pragma warning disable SYSLIB0032 // Type or member is obsolete
    [HandleProcessCorruptedStateExceptions]
#pragma warning restore SYSLIB0032 // Type or member is obsolete
    [SecurityCritical]
    private bool ImportFile(string filepath)
    {
        bool isIfc = false;
#pragma warning disable CS1058 // A previous catch clause already catches all exceptions
        try
        {
            using var odTvFactoryId = TV_Visualize_Globals.odTvGetFactory();

            using var importparam = GetImportParams(filepath, ref isIfc);
            importparam.setFilePath(filepath);

            if (System.IO.Path.GetExtension(filepath) == ".vsfx")
                TvDatabaseId = odTvFactoryId.readVSFX(filepath);
            else
                TvDatabaseId = odTvFactoryId.importFile(importparam);
        }
        catch (AccessViolationException ex)
        {
            MessageBox.Show($"Import failed : {ex.Message}", "Cancellation", MessageBoxButtons.OK);
            //Environment.Exit(1);
        }
        catch (SEHException ex)
        {
            MessageBox.Show($"Import failed : {ex.Message}", "Cancellation", MessageBoxButtons.OK);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Import failed : {ex.Message}", "Cancellation", MessageBoxButtons.OK);
        }
        catch
        {
            MessageBox.Show($"Import failed", "Cancellation", MessageBoxButtons.OK);
        }
#pragma warning restore CS1058 // A previous catch clause already catches all exceptions
        return isIfc;
    }

    private void EmitFileLoadedEvents()
    {
        ViewControl?.SetFileLoaded(true, FilePath, (statusText) => ServiceFactory.EventSrv.GetEvent<AppStatusTextChanged>().Publish(statusText));
        ServiceFactory.EventSrv.GetEvent<CadModelLoadedEvent>().Publish(FilePath);
    }

    public uint GetCadDbUnitAsCadFileUnit(OdTvDatabase odTvDatabase)
    {
        //Set model units in the database
        using var modelsIterator = odTvDatabase.getModelsIterator();
        for (; !modelsIterator.done(); modelsIterator.step())
        {
            using var odTvModelId = modelsIterator.getModel();
            using var odTvModel = odTvModelId.openObject(OdTv_OpenMode.kForRead);
            if (!odTvModelId.isNull())
            {
                var modelUnits = odTvModel.getUnits();
                double userDefCoef = 0;
                if (modelUnits == OdTv_Units.kUserDefined)
                {
                    modelUnits = odTvModel.getUnits(out userDefCoef);
                }
                var modelSurveyUnits = UnitsValueConverter.MapOdaUnitsToSurveyUnits(modelUnits, userDefCoef);
                ServiceFactory.AppSettings = ServiceFactory.AppSettings with { CadFileUnit = modelSurveyUnits };
                return (uint)modelSurveyUnits;
            }
        }
        return (uint)UnitsValue.kUnitsMeters;
    }

    private void SetCadDbUnitAsCadFileUnit(OdTvDatabase odTvDatabase)
    {
        SurveyUnits surveyUnit = ServiceFactory.AppSettings.CadImportUnit;
        CadUnits = UnitsValueConverter.ConvertUnits(surveyUnit, out var isMetric);

        //Set model units in the database
        using var modelsIterator = odTvDatabase.getModelsIterator();
        for (; !modelsIterator.done(); modelsIterator.step())
        {
            using var odTvModelId = modelsIterator.getModel();
            using var odTvModel = odTvModelId.openObject(OdTv_OpenMode.kForWrite);
            if (!odTvModelId.isNull()) 
            {
                var tvUnits = UnitsValueConverter.MapHiltiUnitsToOda(CadUnits);
                if (tvUnits != 0)
                {
                    odTvModel.setUnits(tvUnits);
                }
                else
                {
                    // As US_Feet and other un available units in Units enum are treated as userdefined in visualize
                    // have to set related coef value along with units, as of now HCL is supporting US_Feet so setting US_Feet coef value
                    odTvModel.setUnits(tvUnits, CadModelConstants.UsFeetCoefValue);
                }
            }

        }

        UnitConverter.MapUnitsToMetersConversionFactor =
            UnitsValueConverter.MapUnitsToMetersConversion(CadUnits);
        UnitConverter.MetersToMapUnitsConversionFactor =
            UnitsValueConverter.MetersToMapUnitsConversion(CadUnits);
    }

    private void SetFrozenLayersVisible(OdTvDatabase odTvDatabase)
    {
        if (!ServiceFactory.AppSettings.SetFrozenLayersVisible) return;

        using var layersIt = odTvDatabase.getLayersIterator();
        for (; !layersIt.done(); layersIt.step())
        {
            using var layer = layersIt.getLayer().openObject(OdTv_OpenMode.kForWrite);
            if (layer.getTotallyInvisible())
            {
                layer.setTotallyInvisible(false);
            }
        }
    }

    public void ConfigureViewSettings(OdTvGsViewId bitmapDeviceViewId)
    {
        using var viewOpen = bitmapDeviceViewId.openObject(OdTv_OpenMode.kForWrite);
        viewOpen.setDefaultLightingIntensity(1.25);

        using var color = new OdTvColorDef(CadModelConstants.LightColor.R, CadModelConstants.LightColor.G,
                                     CadModelConstants.LightColor.B);
        viewOpen.setDefaultLightingColor(color);
        viewOpen.enableDefaultLighting(true, OdTvGsView_DefaultLightingType.kTwoLights);
        using var db = viewOpen.getDatabase().openObject(OdTv_OpenMode.kForWrite);
        using var background = db.createBackground("Gradient", OdTvGsViewBackgroundId_BackgroundTypes.kGradient);

        using var bg = background.openAsGradientBackground(OdTv_OpenMode.kForWrite);
        if (bg != null)
        {
            bg.setColorTop(new OdTvColorDef(CadModelConstants.GradientColorTop.R, CadModelConstants.GradientColorTop.G, CadModelConstants.GradientColorTop.B));
            bg.setColorMiddle(new OdTvColorDef(CadModelConstants.GradientColorMiddle.R, CadModelConstants.GradientColorMiddle.G, CadModelConstants.GradientColorMiddle.B));
            bg.setColorBottom(new OdTvColorDef(CadModelConstants.GradientColorBottom.R, CadModelConstants.GradientColorBottom.G, CadModelConstants.GradientColorBottom.B));
            viewOpen.setBackground(background);
        }
        viewOpen.setMode(OdTvGsView_RenderMode.k2DOptimized);
    }
    private OdTvBaseImportParams GetImportParams(string filePath, ref bool isIfc)
    {
        OdTvBaseImportParams importParams = null;
        string ext = System.IO.Path.GetExtension(filePath);
        if (ext != null)
        {
            ext = ext.ToLower();
            if (ext == ".dwg" || ext == ".dxf")
            {
                importParams = new OdTvDwgImportParams();
                OdTvDwgImportParams dwgPmtrs = importParams as OdTvDwgImportParams;
                dwgPmtrs.setDCRect(new OdTvDCRect(0, (int)_widthResized, (int)_heightResized, 0));
                dwgPmtrs.setObjectNaming(ServiceFactory.AppSettings.DwgSetObjectNaming);
                dwgPmtrs.setStoreSourceObjects(ServiceFactory.AppSettings.DwgSetStoreSourceObjects);
                dwgPmtrs.setFeedbackForChooseCallback(null);
                dwgPmtrs.setImportFrozenLayers(ServiceFactory.AppSettings.DwgSetImportFrozenLayers);
                dwgPmtrs.setClearEmptyObjects(ServiceFactory.AppSettings.DwgSetClearEmptyObjects);
                dwgPmtrs.setNeedCDATree(ServiceFactory.AppSettings.DwgSetNeedCdaTree);
                dwgPmtrs.setNeedCollectPropertiesInCDA(ServiceFactory.AppSettings.DwgSetNeedCollectPropertiesInCda);
            }
            else if (ext == ".ifc")
            {
                var tvIfcImportParams = new OdTvIfcImportParams();
                tvIfcImportParams.setNeedCDATree(ServiceFactory.AppSettings.IfcSetNeedCdaTree);
                tvIfcImportParams.setNeedCollectPropertiesInCDA(ServiceFactory.AppSettings.IfcSetNeedCollectPropertiesInCda);
                isIfc = true;
                importParams = tvIfcImportParams;
            }
            else if (ext == ".obj")
                importParams = new OdTvObjImportParams();
            else if (ext == ".stl")
                importParams = new OdTvStlImportParams();

            else if (ext == ".vsf" || ext == ".vsfx")
            {
                importParams = new OdTvBaseImportParams();
            }
        }
        return importParams;
    }
    private static void PCallback(OdTvFilerFeedbackForChooseObject argument)
    {
        var contexts = argument.getFilerFeedbackItemForChooseArrayPtr();
        //For IFC, We only need Body context to be chosen for import as the default setting to avoid unnecessary artifacts
        foreach (var context in contexts)
        {
            var name = context.m_strName;
            if (name == null || name == HiltiCadConstants.MainContext || name.Contains(HiltiCadConstants.BodyContext))
            {
                continue;
            }
            context.m_bChosen = false;
        }

        //if no contexts are set then set the first one as default
        if (!contexts.Any(c => c.m_bChosen) && contexts.Count > 0)
        {
            contexts[0].m_bChosen = true;
        }
    }

    private void Init()
    {
        if (TvDatabaseId == null || TvGsDeviceId == null)
            return;
        MemoryTransaction mtr = _mm.StartTransaction();
        OdTvDatabase pDb = TvDatabaseId.openObject(OdTv_OpenMode.kForWrite);
        _tvDraggersModelId = pDb.createModel("Draggers", OdTvModel_Type.kDirect, false);
        OdTvSelectDragger selectDragger = new OdTvSelectDragger(this, _tvActiveModelId, TvGsDeviceId, _tvDraggersModelId);
        _dragger = selectDragger;
        DraggerResult res = _dragger.Start(null, TvActiveViewport, null, Wcs);
        ActionAfterDragger(res);

        // find service models

        // set projection button
        OdTvGsView view = TvGsDeviceId.openObject().viewAt(TvActiveViewport).openObject();

        _mm.StopTransaction(mtr);
    }
    #endregion

    #region User Mouse Events
    private MouseButtonState _mouseDown = MouseButtonState.Released;

    public void MouseDown(MouseButtonEventArgs e, Point position)
    {
        if (_dragger is OdTvOrbitDragger)
        {
            OnOffViewCube(false);
        }
        _mouseDown = e.MouseDevice.LeftButton == MouseButtonState.Pressed ? MouseButtonState.Pressed :
            e.MouseDevice.RightButton == MouseButtonState.Pressed ? MouseButtonState.Pressed : MouseButtonState.Released;

        if (_dragger == null) return;

        // activation first
        DraggerResult res = _dragger.Activate();
        ActionAfterDragger(res);
        res = _dragger.NextPoint((int)position.X, (int)position.Y);
        ActionAfterDragger(res);
        if (ServiceFactory.AppSettings.Interactivity)
        {
            //start Interactivity
            using var odTvGsDevice = TvGsDeviceId.openObject();
            using var odTvGsView = odTvGsDevice.viewAt(TvActiveViewport).openObject(OdTv_OpenMode.kForWrite);
            odTvGsView.beginInteractivity(ServiceFactory.AppSettings.InteractiveFps);
        }
    }

    public void MouseMove(System.Windows.Input.MouseEventArgs e, Point position, UserInteraction userInteraction)
    {
        if (_dragger != null)
        {
            if (_mouseDown == MouseButtonState.Pressed)
            {
                DraggerResult res = _dragger.Drag((int)position.X, (int)position.Y);
                //Debug.WriteLine("4-AtvViewer-MouseMoveEvent");
                if(userInteraction == UserInteraction.Orbiting)
                {
                    UpdateDefaultModelModelingMatrix();
                    //OK : Arrow rotates properly 
                    _hclPrism?.UpdateViewTransformation();
                    _hclStation?.UpdateViewTransformation();
                    using var _ = CadProfiler.LogFps($"Point Tx - # of Points {HclPointContainer.PointListCount}", "Mouse/Touch Move - Orbiting");
                    OptimizePointTransformations();
                    UpdateCadView();
                }
                else
                {
                    ActionAfterDragger(res);
                }
            }
        }
    }

    private void OptimizePointTransformations()
    {
        if (_hclPointContainer == null)
        {
            return;
        }
        if (ServiceFactory.AppSettings.EnablePointOptimization)
        {
            _pointMoveCycle++;
            if (ServiceFactory.AppSettings.UseDynamicOptimization)
            {
                var countOfPointCycle = HclPointContainer.PointListCount / 1000 + 1;
                if ((_pointMoveCycle % countOfPointCycle) == 0)
                {
                    DoPointTransformation();
                }
            }
            else if (ServiceFactory.AppSettings.RenderEveryXCycle == 0)
            {
                return;
            }
            else if ((_pointMoveCycle % ServiceFactory.AppSettings.RenderEveryXCycle) == 0)
            {
                // Do fixed cycle point transformation
                DoPointTransformation();
            }
        }
        else
        {
            DoPointTransformation();
        }
    }

    private void DoPointTransformation()
    {
        _pointMoveCycle = 0;
        _hclPointContainer?.UpdateTransformations(GetPointScaleFactor());
    }

    private void UpdateDefaultModelModelingMatrix()
    {
        if (TvDefaultModelId is null)
        {
            return;
        }
        //Find Entity under parent Model
        using var modelObject = TvDefaultModelId.openObject(OdTv_OpenMode.kForWrite);
        var foundEntityId = modelObject.findEntity(InnerRectangleEntityHandle);

        //Get Inverse Camera Matrix
        using var odTvGsDevice = TvGsDeviceId.openObject();
        using var odTvGsViewId = odTvGsDevice.viewAt(TvActiveViewport);
        using var odTvGsView = odTvGsViewId.openObject(OdTv_OpenMode.kForWrite);
        using var xEyeToWorld = odTvGsView.eyeToWorldMatrix();
        //Set rotation matrix for the polygon Entity which should be facing camera
        modelObject.setModelingMatrix(foundEntityId, xEyeToWorld.setTranslation(new OdGeVector3d(0, 0, 0)), true);
    }

    public void MouseUp(MouseButtonEventArgs e)
    {
        _mouseDown = MouseButtonState.Released;
        if (ServiceFactory.AppSettings.Interactivity)
        {
            //start Interactivity
            using var odTvGsDevice = TvGsDeviceId.openObject();
            using var odTvGsView = odTvGsDevice.viewAt(TvActiveViewport).openObject(OdTv_OpenMode.kForWrite);
            odTvGsView.endInteractivity();
        }
        UpdateCadView();
    }

    public void MouseWheel(MouseWheelEventArgs e)
    {
        bool zoomIn = e.Delta > 0;
        try
        {
            Zoom(zoomIn ? ZoomType.ZoomIn : ZoomType.ZoomOut);
        }
        finally
        {
        }
    }

    public OdTvExtendedView GetActiveTvExtendedView()
    {
        OdTvExtendedView exView = null;
        //MemoryTransaction mtr = MM.StartTransaction();
        using var odTvGsDevice = TvGsDeviceId.openObject();
        OdTvGsViewId viewId = odTvGsDevice.viewAt(TvActiveViewport);
        if (viewId.isNull())
            return null;
        OdTvResult rc = OdTvResult.tvOk;
        using var view = viewId.openObject();
        ulong handle = view.getDatabaseHandle(ref rc);

        if (_extendedViewDict.ContainsKey(handle))
            exView = _extendedViewDict[handle];
        else
        {
            exView = new OdTvExtendedView(viewId);
            //exView.setZoomScale(VM.ZoomStep);
            exView.setZoomScale(1.1);
            exView.setAnimationDuration(0.9);

            if (view != null)
            {
                using var lastExt = new OdGeBoundBlock3d();
                if (view.getLastViewExtents(lastExt))
                    exView.setViewExtentsForCaching(lastExt);
            }

            _extendedViewDict.Add(handle, exView);
        }

        //MM.StopTransaction(mtr);

        return exView;
    }
    public void SetAnimation(OdTvAnimation animation)
    {
        _animation = animation;
        if (_animation != null)
            _animation.start();
        else if (_dragger != null)
            _dragger.NotifyAboutViewChange(DraggerViewChangeType.ViewChangeFull);
    }
    #endregion

    #region Navigation Commands

    public void Zoom(ZoomType type)
    {
        if (TvDatabaseId == null || TvGsDeviceId == null)
            return;

        //VM.UncheckDraggersBtns();

        //MemoryTransaction mtr = MM.StartTransaction();

        OdTvExtendedView exView = GetActiveTvExtendedView();
        if (exView == null)
            return;
        switch (type)
        {
            case ZoomType.ZoomIn:
                exView.zoomIn();
                break;
            case ZoomType.ZoomOut:
                exView.zoomOut();
                break;
            case ZoomType.ZoomExtents:
                {
                    //using var _ = CadProfiler.Log("Time Elapsed", "ZoomExtents");
                    //ServiceFactory.Logger.LogInformation("ZoomExtents");
                    exView.zoomToExtents();
                    //update cached extents if need
                    using var lastExt = new OdGeBoundBlock3d();
                    if (!exView.getCachedExtents(lastExt))
                    {
                        var viewId = exView.getViewId();
                        using var view = viewId.openObject();
                        if (view.getLastViewExtents(lastExt))
                            exView.setViewExtentsForCaching(lastExt);
                    }
                    break;
                }
            default:
                throw new ArgumentOutOfRangeException(type.ToString() + " (" + type.GetType().ToString() + ")", type, null);
        }

        exView.setViewType(OdTvExtendedView_e3DViewType.kCustom);

        if (_dragger != null)
            _dragger.NotifyAboutViewChange(DraggerViewChangeType.ViewChangeZoom);

        UpdateHclZoomTransformations();
        UpdateCadView(true);

        //MM.StopTransaction(mtr);
    }
    public void Pan()
    {
        if (TvDatabaseId == null || TvGsDeviceId == null)
            return;
        ODA.Draggers.OdTvDragger newDragger = new OdTvPanDragger(TvGsDeviceId, _tvDraggersModelId);
        StartDragger(newDragger, true);
    }
    public void Orbit()
    {
        if (TvDatabaseId == null || TvGsDeviceId == null)
            return;

        //             if (_dragger != null && (_dragger as OdTvSelectDragger) == null)
        //                 FinishDragger();

        ODA.Draggers.OdTvDragger newDragger = new OdTvOrbitDragger(TvGsDeviceId, _tvDraggersModelId);
        StartDragger(newDragger, true);
        Debug.WriteLine("1-Orbit");
        //DisableMarkups();
    }

    public void ZoomToArea()
    {
        if (TvDatabaseId == null || TvGsDeviceId == null)
            return;

        //             if (_dragger != null && (_dragger as OdTvSelectDragger) == null)
        //                 FinishDragger();

        ODA.Draggers.OdTvDragger newDragger = new OdTvOrbitDragger(TvGsDeviceId, _tvDraggersModelId);
        StartDragger(newDragger, true);
        Debug.WriteLine("1-Orbit");
        //DisableMarkups();
    }
    #endregion

    #region Views & Styles
    public void Set3DView(OdTvExtendedView_e3DViewType type)
    {
        //MemoryTransaction mtr = MM.StartTransaction();

        OdTvExtendedView exView = GetActiveTvExtendedView();
        if (exView == null)
            return;

        // set view type
        exView.setViewType(type);

        //update cached extents if need
        using var lastExt = new OdGeBoundBlock3d();
        if (!exView.getCachedExtents(lastExt))
        {
            var viewId = exView.getViewId();
            using var view = viewId.openObject();
            if (view.getLastViewExtents(lastExt))
                exView.setViewExtentsForCaching(lastExt);
        }
        UpdateDefaultModelModelingMatrix();

        var scaleFactor = GetScaleFactor();
        _hclPrism?.ScaleModelAtLocation(scaleFactor, HclPrismLocation);
        _hclPrism?.UpdateViewTransformation();
        _hclStation?.ScaleModelAtLocation(scaleFactor, HclStationLocation);
        _hclStation?.UpdateViewTransformation();
        _hclPointContainer?.UpdateTransformations(GetPointScaleFactor());
        UpdateCadView();

        UpdateWcsView();
        //MM.StopTransaction(mtr);
        if (ServiceFactory.AppSettings.ShowCube)
        {
            OnOffViewCube(true);
        }
    }

    private void UpdateWcsView()
    {
        using var parentView = Wcs.GetParentView(OdTv_OpenMode.kForRead);
        var parentViewPosition = parentView.position();
        var targetTranslation = OdGeMatrix3d.translation(-parentView.target().asVector());

        using var wcsView = Wcs.GetWcsView(OdTv_OpenMode.kForWrite);

        wcsView.setView(parentViewPosition.transformBy(targetTranslation),
                            OdGePoint3d.kOrigin, parentView.upVector(), 1, 1);
        wcsView.setMode(OdTvGsView_RenderMode.kGouraudShaded);
        wcsView.zoom(4.2);
        OdGePoint2d lowerLeft = new OdGePoint2d();
        OdGePoint2d upperRight = new OdGePoint2d();
        parentView.getViewport(lowerLeft, upperRight);
        upperRight.x = lowerLeft.x + 1.9;
        upperRight.y = lowerLeft.y + 0.23;
        wcsView.setViewport(lowerLeft, upperRight);
        UpdateCadView();
    }

    public OdGeVector3d GetEyeDirection()
    {
        OdGeVector3d eyeDir = new OdGeVector3d(OdGeVector3d.kIdentity);
        MemoryTransaction mtr = _mm.StartTransaction();

        OdTvGsView pView = GetActiveTvExtendedView().getViewId().openObject();
        if (pView != null)
            eyeDir = pView.position() - pView.target();

        _mm.StopTransaction(mtr);
        return eyeDir;
    }
    public void SetRenderMode(OdTvGsView_RenderMode renderMode)
    {
        //MemoryTransaction mtr = MM.StartTransaction();
        if (TvGsDeviceId != null && !TvGsDeviceId.isNull())
        {
            using var odTvGsDevice = TvGsDeviceId.openObject();
            using var view = odTvGsDevice.viewAt(TvActiveViewport).openObject(OdTv_OpenMode.kForWrite);
            OdTvGsView_RenderMode oldMode = view.mode();
            if (oldMode != renderMode)
            {
                view.setMode(renderMode);

                // set mode for WCS
                //if (WCS != null && ServiceFactory.AppSettings.ShowWCS && WCS.IsNeedUpdateWCS(oldMode, renderMode))
                //    WCS.UpdateWCS();

                //TvDeviceId.openObject().update();
                UpdateCadView();
                ViewControl.SetRenderModeButton(renderMode);
            }
        }
        //MM.StopTransaction(mtr);
    }
    public void SetProjectionType(OdTvGsView_Projection projection)
    {
        MemoryTransaction mtr = _mm.StartTransaction();
        if (TvGsDeviceId != null && !TvGsDeviceId.isNull())
        {
            OdTvGsView view = TvGsDeviceId.openObject().viewAt(TvActiveViewport).openObject(OdTv_OpenMode.kForWrite);
            view.setView(view.position(), view.target(), view.upVector(), view.fieldWidth(), view.fieldHeight(), projection);
            //TvDeviceId.openObject().update();
            UpdateCadView();
        }
        _mm.StopTransaction(mtr);
    }

    public void SetBackgroundColor(Color color)
    {
        MemoryTransaction mtr = _mm.StartTransaction();
        uint iColor = ((uint)(color.R | color.G << 8 | ((color.B) << 16)));
        if (TvGsDeviceId != null && !TvGsDeviceId.isNull())
        {
            OdTvGsDevice dev = TvGsDeviceId.openObject(OdTv_OpenMode.kForWrite);
            dev.setBackgroundColor(iColor);
            dev.update();
        }
        _mm.StopTransaction(mtr);
    }
    #endregion

    #region Regen
    public void Regen(OdTvGsDevice_RegenMode rm)
    //public async void Regen(OdTvGsDevice_RegenMode rm)
    {
        if (TvGsDeviceId == null)
            return;
        MemoryTransaction mtr = _mm.StartTransaction();
        var dev = TvGsDeviceId.openObject();
        // Regen works in thread
        //await Task.Run(async () =>
        //{
        //    dev.regen(rm);
        //    dev.invalidate();
        //    await Task.Delay(3000);
        //});
        dev.regen(rm);
        dev.invalidate();
        //dev.update();
        UpdateCadView();
        _mm.StopTransaction(mtr);
    }

    public void Regen()
    {
        if (TvGsDeviceId == null)
            return;
        MemoryTransaction mtr = _mm.StartTransaction();
        OdTvGsDevice dev = TvGsDeviceId.openObject();
        if (TvActiveViewport > 0)
            dev.viewAt(TvActiveViewport).openObject().regen();
        dev.invalidate();
        //dev.update();
        UpdateCadView();
        _mm.StopTransaction(mtr);
    }


    #endregion

    #region HCL Tools

    public void ShowTool(HclToolType type)
    {
        _cachedViewCenter = GetViewCenter();
        HclStationLocation ??= CadPoint3D.With(_cachedViewCenter);
        HclPrismLocation ??= CadPoint3D.With(_cachedViewCenter);
        using var viewPos = ViewParameters.FromView(this);
        switch (type)
        {
            case HclToolType.PLTStation:
                _hclStation = HclStationBuilder.ShowPltStation(this, HclStationLocation);
                _hclStation?.UpdateViewTransformation();
                break;
            case HclToolType.Prism:
                _hclPrism = HclPrismBuilder.ShowPrism(this, HclPrismLocation);
                _hclPrism?.UpdateViewTransformation();
                break;
            case HclToolType.Points:
                if (_hclPointContainer == null)
                {
                    Set3DView(OdTvExtendedView_e3DViewType.kTop);
                }
                _hclPointContainer = HclPointBuilder.ShowPoints(this);
                _hclPointContainer?.UpdateTransformations(GetPointScaleFactor());
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
        UpdateToolLine();
        UpdateCadView();
    }

    public OdTvGsViewId GetViewId()
    {
        using var odTvGsDevice = TvGsDeviceId.openObject();
        return odTvGsDevice.viewAt(TvActiveViewport);
    }
    public bool IsCadViewRotated() => GetViewId().IsCADRotated();
    public void ChangePrismLocation(Point mousePosition)
    {
        if (_hclPrism == null) return;
        using var oldLocation = CadPoint3D.With(HclPrismLocation!);
        HclPrismLocation?.Dispose();
        HclPrismLocation = UpdateStatusBarCoordinates(mousePosition);
        _hclPrism?.UpdateLocationDelta(HclPrismLocation, oldLocation);
        UpdateToolLine();
        UpdateCadView();
    }
    public void ChangeStationLocation(Point mousePosition)
    {
        if (_hclStation == null) return;
        using var oldLocation = CadPoint3D.With(HclStationLocation!);
        HclStationLocation?.Dispose();
        HclStationLocation = UpdateStatusBarCoordinates(mousePosition);
        _hclStation?.UpdateLocationDelta(HclStationLocation, oldLocation);
        UpdateToolLine();
        UpdateCadView();
    }

    private void UpdateToolLine()
    {
        if (_hclToolLine is not null && _hclToolLine.TvModelId != null)
        {
            _hclToolLine.Remove(this);
            _hclToolLine = null;
        }
        if (_hclStation != null && _hclPrism != null)
        {
            _hclToolLine = HclToolLineBuilder.ShowToolLine(this, HclStationLocation, HclPrismLocation);
        }
        if (_hclToolLine != null)
        {
            var geometryDataId = _hclToolLine?.VisibleEntityGeometryDict.Values.ToList()[0];
            using var polyLine = geometryDataId.openAsPolyline();
            using var points = new OdGePoint3dVector();
            polyLine.getPoints(points);
            var message = $"Tool Line Wcs: Point 1 = [{points[0].x:0.##} - {points[0].y:0.##} - {points[0].z:0.##}];Tool Line Wcs: Point 2 = [{points[1].x:0.##} - {points[1].y:0.##} - {points[1].z:0.##}]";
            ServiceFactory.EventSrv.GetEvent<ScreenCoordinatesChanged>().Publish(message);
        }
    }

    public CadPoint3D UpdateStatusBarCoordinates(Point mousePosition)
    {
        var screenLocation = CadScreenInfoProvider.DpiScaledPoint(mousePosition);
        var mouseWcsLocation = screenLocation.FromScreenToWorld(GetViewId());
        var message = $"Screen Coordinates:[{mouseWcsLocation.X:#.##} - {mouseWcsLocation.Y:#.##}];World Coordinates:[{mouseWcsLocation.X:0.##} - {mouseWcsLocation.Y:0.##} - {mouseWcsLocation.Z:0.##}]";
        ServiceFactory.EventSrv.GetEvent<ScreenCoordinatesChanged>().Publish(message);
        return mouseWcsLocation;
    }
    public CadPoint3D GetViewCenter()
    {
        if (_cachedViewCenter != null)
        {
            return _cachedViewCenter;
        }
        using var odTvGsViewId = GetViewId();
        using var cachedExtents = new OdGeBoundBlock3d();
        using var odTvGsView = odTvGsViewId.openObject(OdTv_OpenMode.kForWrite);
        odTvGsView.getLastViewExtents(cachedExtents);
        return CadPoint3D.With(cachedExtents.center());
    }

    public (CadPoint3D min, CadPoint3D max) GetViewExtent()
    {
        using var odTvGsViewId = GetViewId();
        using var cachedExtents = new OdGeBoundBlock3d();
        using var odTvGsView = odTvGsViewId.openObject();
        odTvGsView.getLastViewExtents(cachedExtents);
        return (cachedExtents.minPoint(), cachedExtents.maxPoint());
    }
    public double GetScaleFactor()
    {
        using var viewId = GetViewId();
        var radius = viewId.GetPixelScaleFactorAtViewTarget(100);
        var scaleFactor = radius / LastRadius;
        LastRadius = radius;

        var message = $"CR:[{radius:0.##} - LR:[{LastRadius:0.##}] - Scale Factor:[{scaleFactor:0.##}]";
        ServiceFactory.EventSrv.GetEvent<ScaleFactorChanged>().Publish(message);
        return scaleFactor;
    }
    public double GetPointScaleFactor()
    {
        using var viewId = GetViewId();
        var radius = viewId.GetPixelScaleFactorAtViewTarget(100);
        var scaleFactor = radius / PointScaleFactor;

        if (_hclPointContainer !=null && !PointScaleFactorSet)
        {
            PointScaleFactorSet = true;
            PointScaleFactor = scaleFactor;
        }
        return scaleFactor;
    }
    public double GetPrismScaleFactor()
    {
        using var viewId = GetViewId();
        var radius = viewId.GetPixelScaleFactorAtViewTarget(100);
        var scaleFactor = radius / PrismScaleFactor;

        if (_hclPrism != null && !PrismScaleFactorSet)
        {
            PrismScaleFactorSet = true;
            PrismScaleFactor = scaleFactor;
        }
        return scaleFactor;
    }
    #endregion

    #region Draggers
    private void StartDragger(ODA.Draggers.OdTvDragger dragger, bool useCurrentAsPrevious = false)
    {
        DraggerResult res = DraggerResult.NothingToDo;

        if (_dragger == null)
        {
            res = dragger.Start(null, TvActiveViewport, null, Wcs);
        }
        else
        {
            ODA.Draggers.OdTvDragger pPrevDragger = _dragger;
            if (_dragger.HasPrevious())
            {
                DraggerResult resPrev;
                if (useCurrentAsPrevious)
                    _dragger.Finish(out resPrev);
                else
                    pPrevDragger = _dragger.Finish(out resPrev);
                //ActionAfterDragger(res_prev);
            }
            res = dragger.Start(pPrevDragger, TvActiveViewport, null, Wcs);
        }
        // need update active dragger before calling action
        _dragger = dragger;
        ActionAfterDragger(res);
    }
    private void ActionAfterDragger(DraggerResult res)
    {

        if ((res & DraggerResult.NeedUpdateView) != 0)
        {
            //MemoryTransaction mtr = MM.StartTransaction();
            //OdTvGsDevice btmpDev = TvDeviceId.openObject(OpenMode.kForWrite);
            //btmpDev.update();
            //MM.StopTransaction(mtr);
            UpdateCadView();
        }

        if ((res & DraggerResult.NeedUFinishDragger) != 0)
        {
            //FinishDragger();
        }
    }
    public void FinishDragger()
    {
        if (_dragger != null)
        {
            if (_dragger.HasPrevious() && _dragger.CanFinish())
            {
                // release current dragger
                DraggerResult res;
                ODA.Draggers.OdTvDragger prevDragger = _dragger.Finish(out res);
                ActionAfterDragger(res);

                // activate previous dragger
                _dragger = prevDragger;
                //res = _dragger.Start(null, TvActiveViewport, Cursor, WCS);
                ActionAfterDragger(res);
            }
        }
    }
    private void DisableMarkups()
    {
        if (_tvMarkupModelId != null && _dbId != null)
        {
            MemoryTransaction mtr = _mm.StartTransaction();

            OdTvModel pMarkupModel = _tvMarkupModelId.openObject(OdTv_OpenMode.kForWrite);
            if (pMarkupModel == null)
            {
                _mm.StopTransaction(mtr);
                return;
            }
            OdTvEntitiesIterator pIt = pMarkupModel.getEntitiesIterator();
            if (pIt != null && !pIt.done())
            {
                while (!pIt.done())
                {
                    MemoryTransaction mtr2 = _mm.StartTransaction();

                    OdTvEntityId entityId = pIt.getEntity();
                    OdTvEntity pEn = entityId.openObject(OdTv_OpenMode.kForWrite);
                    if (pEn.getName() == OdTvMarkupDragger.NameOfMarkupTempEntity) // if temp entity
                    {
                        pMarkupModel.removeEntity(entityId);
                    }
                    else if (pEn.getVisibility().getType() != OdTvVisibilityDef_VisibilityType.kInvisible)
                    {
                        OdTvGeometryDataIterator pItF = pEn.getGeometryDataIterator();
                        // folds
                        while (!pItF.done())
                        {
                            // objects
                            OdTvEntity pFold = pItF.getGeometryData().openAsSubEntity(OdTv_OpenMode.kForWrite);
                            OdTvGeometryDataIterator pItO = pFold.getGeometryDataIterator();

                            while (!pItO.done())
                            {
                                OdTvGeometryDataId geomId = pItO.getGeometryData();
                                OdTvUserData usrData = geomId.openAsSubEntity(OdTv_OpenMode.kForWrite).getUserData(AppTvId);
                                if (usrData == null)
                                    pFold.removeGeometryData(geomId);

                                pItO.step();
                            }

                            pItF.step();
                        }
                        pEn.setVisibility(new OdTvVisibilityDef(false));
                    }

                    _mm.StopTransaction(mtr2);
                    pIt.step();
                }
            }

            _mm.StopTransaction(mtr);
        }
    }
    #endregion

    #region File commands
    public void SaveFile(string filePath)
    {
        //this.Cursor = Cursors.Wait;
        if (_dbId == null)
        {
            MessageBox.Show("There is no database for the save", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        try
        {
            MemoryTransaction mtr = _mm.StartTransaction();
            OdTvDatabase db = _dbId.openObject(OdTv_OpenMode.kForWrite);
            OdTvResult rc = db.writeFile(filePath);
            _mm.StopTransaction(mtr);
        }
        catch
        {
            MessageBox.Show("Saving of file\n'" + filePath + "'\n was failed", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        //this.Cursor = Cursors.Arrow;
        FilePath = filePath;
    }

    public void CreateNewFile()
    {
        MemoryTransaction mtr = _mm.StartTransaction();
        //this.Cursor = Cursors.Wait;
        OdTvFactoryId factId = TV_Visualize_Globals.odTvGetFactory();
        factId.clearDatabases();
        DatabaseInfo = new TvDatabaseInfo();
        Stopwatch timer = Stopwatch.StartNew();
        timer.Start();
        _dbId = factId.createDatabase();
        try
        {
            OdTvResult rc = OdTvResult.tvCannotOpenFile;
            OdTvDatabase pDatabase = _dbId.openObject(OdTv_OpenMode.kForWrite, ref rc);
            // Create model
            _tvActiveModelId = pDatabase.createModel("Tv_Model", OdTvModel_Type.kMain);
        }
        catch
        {
            MessageBox.Show("Cannot create new file!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        timer.Stop();
        DatabaseInfo.TvCreationTime = timer.ElapsedMilliseconds;

        TvGsDeviceId = CreateNewDevice();
        Init();

        timer.Restart();
        //TvDeviceId.openObject().update();
        UpdateCadView();
        timer.Stop();
        DatabaseInfo.FirstUpdateTime = timer.ElapsedMilliseconds;

        //VM.FileIsExist = true;
        FilePath = "";
        //this.Cursor = Cursors.Arrow;
        //VM.MainWindow.ModelBrowser.Initialize(_dbId, this);
        _mm.StopTransaction(mtr);
    }

    private OdTvGsDeviceId CreateNewDevice()
    {
        MemoryTransaction mtr = _mm.StartTransaction();
        OdTvGsDeviceId newDevId = null;

        try
        {
            //IntPtr wndHndl = new IntPtr(Handle.ToInt32());
            OdTvDCRect rect = new OdTvDCRect(0, _widthResized, _heightResized, 0);
            //newDevId = _dbId.openObject().createDevice("TV_Device", wndHndl, rect, OdTvGsDevice.Name.kOpenGLES2);
            // Open device
            OdTvGsDevice pDevice = newDevId.openObject(OdTv_OpenMode.kForWrite);
            if (pDevice == null)
                return null;

            //                 bool val;
            //                 pDevice.getOption(OdTvGsDevice.Options.kUseVisualStyles, out val);

            // Create view
            OdTvGsViewId newViewId = pDevice.createView("TV_View");
            TvActiveViewport = 0;

            // Add view to device
            pDevice.addView(newViewId);

            // Add current model to the view
            OdTvGsView viewPtr = newViewId.openObject(OdTv_OpenMode.kForWrite);

            // Setup view to make it contr directional with the WCS normal
            viewPtr.setView(new OdGePoint3d(0, 0, 1), new OdGePoint3d(0, 0, 0), new OdGeVector3d(0, 1, 0), 1, 1);

            // Add main model to the view
            viewPtr.addModel(_tvActiveModelId);

            // Set current view as active
            viewPtr.setActive(true);

            // Set the render mode
            viewPtr.setMode(OdTvGsView_RenderMode.k2DOptimized);

            newDevId.openObject().onSize(rect);
        }
        catch (Exception e)
        {
            MessageBox.Show("Cannot create device:" + e);
            throw;
        }

        _mm.StopTransaction(mtr);
        return newDevId;
    }

    public void ExportToPdf(string fileName, bool is2D = true)
    {
        MemoryTransaction mtr = _mm.StartTransaction();

        OdTvDatabase db = _dbId.openObject(OdTv_OpenMode.kForWrite);
        if (db == null)
        {
            MessageBox.Show("There is no database for the save!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            _mm.StopTransaction(mtr);
            return;
        }
        //this.Cursor = Cursors.Wait;

        //call method for write the file
        try
        {
            OdTvBaseExportParams exportParams = new OdTvBaseExportParams();
            exportParams.setFilePath(fileName);
            OdTvResult rc = db.exportTo(exportParams);
            if (rc != OdTvResult.tvOk)
            {
                MessageBox.Show("Export of file " + fileName + " was failed!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _mm.StopTransaction(mtr);
                //this.Cursor = Cursors.Arrow;
                return;
            }
        }
        catch
        {
            MessageBox.Show("Export of file " + fileName + " was failed!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            _mm.StopTransaction(mtr);
            //this.Cursor = Cursors.Arrow;
            return;
        }

        //this.Cursor = Cursors.Arrow;
        _mm.StopTransaction(mtr);
    }

    #endregion

    #region Drawing commands

    public void DrawGeometry(string type)
    {
        DisableMarkups();
        ODA.Draggers.OdTvDragger newDragger = null;
        switch (type)
        {
            case "Polyline":
                newDragger = new OdTvPolylineDragger(TvGsDeviceId, _tvDraggersModelId, _tvActiveModelId);
                break;
            case "Ray":
                newDragger = new OdTvRayDragger(TvGsDeviceId, _tvDraggersModelId, _tvActiveModelId);
                break;
            case "XLine":
                newDragger = new OdTvXLineDragger(TvGsDeviceId, _tvDraggersModelId, _tvActiveModelId);
                break;
            case "Circle":
                newDragger = new OdTvCircleDragger(TvGsDeviceId, _tvDraggersModelId, _tvActiveModelId);
                break;
        }

        if (newDragger != null)
            StartDragger(newDragger);
    }

    #endregion

    #region Markups commands

    private void CreateMarkupModel()
    {
        if (_tvMarkupModelId == null)
        {
            MemoryTransaction mtr = _mm.StartTransaction();
            _tvMarkupModelId = _dbId.openObject(OdTv_OpenMode.kForWrite).createModel(OdTvMarkupDragger.NameOfMarkupModel, OdTvModel_Type.kDirect, true);
            _mm.StopTransaction(mtr);
        }
    }

    public void DrawRectMarkup()
    {
        CreateMarkupModel();
        ODA.Draggers.OdTvDragger newDragger = new ODA.Draggers.Markups.OdTvRectMarkupDragger(TvGsDeviceId, _tvMarkupModelId);
        StartDragger(newDragger);
    }

    public void DrawCircMarkup()
    {
        CreateMarkupModel();
        ODA.Draggers.OdTvDragger newDragger = new ODA.Draggers.Markups.OdTvCircleMarkupDragger(TvGsDeviceId, _tvMarkupModelId);
        StartDragger(newDragger);
    }

    public void DrawHandleMarkup()
    {
        CreateMarkupModel();
        ODA.Draggers.OdTvDragger newDragger = new ODA.Draggers.Markups.OdTvHandleMarkupDragger(TvGsDeviceId, _tvMarkupModelId);
        StartDragger(newDragger);
    }

    public void DrawCloudMarkup()
    {
        CreateMarkupModel();
        ODA.Draggers.OdTvDragger newDragger = new ODA.Draggers.Markups.OdTvCloudMarkupDragger(TvGsDeviceId, _tvMarkupModelId);
        StartDragger(newDragger);
    }

    public void DrawTextMarkup()
    {
        CreateMarkupModel();
        ODA.Draggers.OdTvDragger newDragger = new ODA.Draggers.Markups.OdTvTextMarkupDragger(TvGsDeviceId, _tvMarkupModelId);
        StartDragger(newDragger);
    }

    public void SaveMarkup()
    {
        MemoryTransaction mtr = _mm.StartTransaction();

        if (_tvMarkupModelId == null || _tvMarkupModelId.openObject().getEntitiesIterator().done())
        {
            MessageBox.Show("Markup model is empty!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (_tvMarkupModelId.openObject().getEntitiesIterator().done())
        {
            MessageBox.Show("No one marup for save!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // find current active entity
        OdTvEntitiesIterator it = _tvMarkupModelId.openObject().getEntitiesIterator();
        OdTvEntityId activeEntityId = null;
        while (!it.done())
        {
            OdTvEntityId curEnId = it.getEntity();
            OdTvEntity curEn = curEnId.openObject();
            if (curEn.getName() == OdTvMarkupDragger.NameOfMarkupTempEntity || curEn.getVisibility().getType() !=
                OdTvVisibilityDef_VisibilityType.kInvisible)
            {
                activeEntityId = curEnId;
                break;
            }
            it.step();
        }

        if (activeEntityId == null)
        {
            MessageBox.Show("No one marup for save!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        FinishDragger();

        SaveMarkupDialog dlg = new SaveMarkupDialog(_tvMarkupModelId, activeEntityId, TvGsDeviceId.openObject().viewAt(TvActiveViewport));
        dlg.ShowDialog();

        _mm.StopTransaction(mtr);
    }

    public void LoadMarkup()
    {
        MemoryTransaction mtr = _mm.StartTransaction();

        if (_tvMarkupModelId == null || _tvMarkupModelId.openObject().getEntitiesIterator().done())
        {
            MessageBox.Show("Markup model is empty!\nPlease create markup.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        FinishDragger();

        LoadMarkupDialog dlg = new LoadMarkupDialog(_tvMarkupModelId, TvGsDeviceId.openObject().viewAt(TvActiveViewport), this);
        if (dlg.ShowDialog() == true)
            //Invalidate();
            UpdateCadView();

        _mm.StopTransaction(mtr);
    }

    #endregion

    #region Appearance commands

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

    #endregion

    #region Selection methods

    public void AddBoldItem(TvTreeItem node)
    {
        ClearSelectedNodes();
        node.SetBold(true);
        if (!SelectedNodes.Contains(node))
            SelectedNodes.Add(node);
    }

    public void ClearSelectedNodes()
    {
        for (int i = 0; i < SelectedNodes.Count; i++)
        {
            TvTreeItem node = SelectedNodes[i];
            node.SetBold(false);
        }
    }

    public void ClearSelectionSet()
    {
        if (SelectionSet == null || TvGsDeviceId == null || TvGsDeviceId.isNull())
            return;
        OdTvExtendedView exView = GetActiveTvExtendedView();
        if (exView == null)
            return;

        MemoryTransaction mtr = _mm.StartTransaction();

        OdTvGsView view = exView.getViewId().openObject();
        if (view == null)
            return;

        OdTvSelectionSetIterator pIter = SelectionSet.getIterator();
        for (; pIter != null && !pIter.done(); pIter.step())
        {
            //get entity
            OdTvEntityId id = pIter.getEntity();
            //get sub item
            OdTvSubItemPath path = new OdTvSubItemPath();
            pIter.getPath(path);
            //perform highlight
            view.highlight(id, path, false);
        }

        SelectionSet.Dispose();
        SelectionSet = null;

        //Invalidate();
        UpdateCadView();

        _mm.StopTransaction(mtr);
    }

    public void AddEntityToSet(OdTvEntityId enId)
    {
        if (enId == null || enId.isNull())
            return;
        OdTvExtendedView exView = GetActiveTvExtendedView();
        if (exView == null)
            return;

        ClearSelectionSet();

        OdTvSelectionOptions opt = new OdTvSelectionOptions();
        opt.setLevel(OdTvSelectionOptions_Level.kEntity);
        opt.setMode(OdTvSelectionOptions_Mode.kPoint);
        SelectionSet = OdTvSelectionSet.createObject(opt);
        SelectionSet.appendEntity(enId);

        MemoryTransaction mtr = _mm.StartTransaction();
        OdTvGsView view = exView.getViewId().openObject();
        if (view == null)
            return;

        OdTvSelectionSetIterator pIter = SelectionSet.getIterator();
        for (; pIter != null && !pIter.done(); pIter.step())
        {
            //get entity
            OdTvEntityId id = pIter.getEntity();
            //get sub item
            OdTvSubItemPath path = new OdTvSubItemPath();
            pIter.getPath(path);
            //perform highlight
            view.highlight(id, path, true);
        }

        _mm.StopTransaction(mtr);

        //Invalidate();
        UpdateCadView();
    }

    #endregion

    #region Cutting planes

    public void ShowSectioningOptions()
    {
        CuttingPlaneOptionsDialog dlg = new CuttingPlaneOptionsDialog(this);
        dlg.ShowDialog();
    }

    public void ApplySectioningOptions()
    {
        if (TvGsDeviceId == null || TvGsDeviceId.isNull())
            return;
        MemoryTransaction mtr = _mm.StartTransaction();

        OdTvGsView pView = GetActiveTvExtendedView().getViewId().openObject(OdTv_OpenMode.kForWrite);
        if (pView == null)
        {
            _mm.StopTransaction(mtr);
            return;
        }

        uint iOldColor = 0;
        bool bOldFillingEnabled = pView.getCuttingPlaneFillEnabled(out iOldColor);

        bool bNewFillingEnabled = SectioningOptions.IsFilled;
        uint iNewColor = SectioningOptions.FillingColor;

        if (bNewFillingEnabled != bOldFillingEnabled || iNewColor != iOldColor)
            pView.setEnableCuttingPlaneFill(bNewFillingEnabled, iNewColor);

        uint iOldPatternColor = 0;
        OdTvGsView_CuttingPlaneFillStyle oldFillingPatternStyle = OdTvGsView_CuttingPlaneFillStyle.kCheckerboard;
        bool bOldFillingPatternEnabled = pView.getCuttingPlaneFillPatternEnabled(out oldFillingPatternStyle, out iOldPatternColor);

        bool bNewFillingPatternEnabled = SectioningOptions.FillingPatternEnabled;
        OdTvGsView_CuttingPlaneFillStyle newFillingPatternStyle = SectioningOptions.FillingPaternStyle;
        uint iNewFillingPatternColor = SectioningOptions.FillingPatternColor;

        if (bNewFillingPatternEnabled != bOldFillingPatternEnabled
          || newFillingPatternStyle != oldFillingPatternStyle
          || iNewFillingPatternColor != iOldPatternColor)
            pView.setCuttingPlaneFillPatternEnabled(bNewFillingPatternEnabled, newFillingPatternStyle, iNewFillingPatternColor);

        OdTvGsDevice pDevice = TvGsDeviceId.openObject();
        if (pDevice != null)
        {
            //pDevice.invalidate();
            //pDevice.update();
            UpdateCadView();
        }

        _mm.StopTransaction(mtr);
    }

    public void OnAppearSectioningPanel(bool bAppear)
    {
        if (bAppear)
        {
            if (SectioningOptions.IsShown)
            {
                if (!(_dragger is OdTvCuttingPlaneDragger))
                {
                    ODA.Draggers.OdTvDragger pNewDragger = new ODA.Draggers.OdTvCuttingPlaneDragger(TvGsDeviceId, _tvDraggersModelId, this);
                    if (pNewDragger != null)
                        StartDragger(pNewDragger, true);
                    //Invalidate();
                    UpdateCadView();
                }
            }
        }
        else
        {
            if (_dragger is OdTvCuttingPlaneDragger)
            {
                OdTvCuttingPlaneDragger cuttingPlaneDragger = (OdTvCuttingPlaneDragger)_dragger;
                if (cuttingPlaneDragger != null)
                {
                    cuttingPlaneDragger.IsCanFinish = true;
                    FinishDragger();
                    //Invalidate();
                    UpdateCadView();
                }
            }
        }
    }

    public bool ShowCuttingPlanes()
    {
        MemoryTransaction mtr = _mm.StartTransaction();
        SectioningOptions.IsShown = !SectioningOptions.IsShown;

        bool bRet = false;

        OdTvGsView pActiveView = GetActiveTvExtendedView().getViewId().openObject(OdTv_OpenMode.kForWrite);
        OdTvGsDevice pDevice = TvGsDeviceId.openObject(OdTv_OpenMode.kForWrite);
        if (pActiveView == null || pDevice == null)
        {
            _mm.StopTransaction(mtr);
            return bRet;
        }

        if (SectioningOptions.IsShown)
        {
            //here we should add the view to the device and to the active view
            pDevice.addView(_cuttingPlanesViewId);
            //add view as sibling to an active
            pActiveView.addSibling(_cuttingPlanesViewId);
            //create sectioning planes geometry
            for (int i = 0; i < pActiveView.numCuttingPlanes(); i++)
            {
                DrawCuttingPlane(i, pActiveView);
            }
            // create and start new dragger
            ODA.Draggers.OdTvDragger pNewDragger = new ODA.Draggers.OdTvCuttingPlaneDragger(TvGsDeviceId, _tvDraggersModelId, this);
            if (pNewDragger != null)
                StartDragger(pNewDragger, true);

            //pDevice.invalidate();
            UpdateCadView();

            bRet = true;
        }
        else
        {
            if (_dragger is OdTvCuttingPlaneDragger)
            {
                // finish the dragger
                OdTvCuttingPlaneDragger cuttingPlaneDragger = (OdTvCuttingPlaneDragger)_dragger;
                cuttingPlaneDragger.IsCanFinish = true;
                FinishDragger();
            }

            // remove geometry for the sectioning planes
            if (!_cuttingPlanesModelId.isNull())
            {
                OdTvModel pMoveModel = _cuttingPlanesModelId.openObject(OdTv_OpenMode.kForWrite);
                if (pMoveModel != null)
                    pMoveModel.clearEntities();
            }

            // remove view as sibling from an active
            pActiveView.removeSibling(_cuttingPlanesViewId);
            // remove view from device
            pDevice.removeView(_cuttingPlanesViewId);
            bRet = true;
        }

        //Invalidate();
        UpdateCadView();
        _mm.StopTransaction(mtr);
        return bRet;
    }

    protected void DrawCuttingPlane(int index, OdTvGsView pView, bool bNeedNotifyDragger = false)
    {
        if (pView == null)
            return;
        MemoryTransaction mtr = _mm.StartTransaction();

        OdGePlane cuttingPlane = new OdGePlane();
        OdTvResult rc = pView.getCuttingPlane((uint)index, cuttingPlane);
        if (rc != OdTvResult.tvOk)
        {
            _mm.StopTransaction(mtr);
            return;
        }

        OdTvModel pCuttingPlaneModel = _cuttingPlanesModelId.openObject(OdTv_OpenMode.kForWrite);
        // create cutting plane entity
        OdTvEntityId cuttingPlanesEntityId = pCuttingPlaneModel.appendEntity("$_CUTTINGPLANE_ENTITY" + index);
        //set a few parameters to the cutting plane
        OdTvEntity pCuttingPlanesEntity = cuttingPlanesEntityId.openObject(OdTv_OpenMode.kForWrite);
        pCuttingPlanesEntity.setColor(new OdTvColorDef(175, 175, 175));
        pCuttingPlanesEntity.setLineWeight(new OdTvLineWeightDef(OdTvCuttingPlaneDragger.OdTvCuttingplaneEdgeDefaultLineweight));
        pCuttingPlanesEntity.setTransparency(new OdTvTransparencyDef(0.8));
        pCuttingPlanesEntity.addViewDependency(_cuttingPlanesViewId);

        IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(index));
        Marshal.WriteInt32(ptr, 0, index);
        OdTvByteUserData data = new OdTvByteUserData(ptr, sizeof(int), OdTvByteUserData_Ownership.kCopyOwn, false);

        pCuttingPlanesEntity.appendUserData(data, AppTvId);

        // Calculate points for cutting plane shell
        OdGePoint3d origin = new OdGePoint3d();
        OdGeVector3d uAxis = new OdGeVector3d();
        OdGeVector3d vAxis = new OdGeVector3d();
        cuttingPlane.get(origin, uAxis, vAxis);

        // Get max distance between extents
        double cuttingPlaneSize = GetMainModelExtentsDistance() / 2d;

        OdGePoint3dVector points = new OdGePoint3dVector();

        OdGeMatrix3d transformMatrix = new OdGeMatrix3d();
        // 0
        OdGeVector3d moveVector0 = -vAxis - uAxis;
        moveVector0 = moveVector0 * cuttingPlaneSize * OdTvCuttingPlaneDragger.OdTvCuttingplaneSizeCoeff;
        transformMatrix.setToIdentity();
        transformMatrix.setToTranslation(moveVector0);
        OdGePoint3d point0 = new OdGePoint3d(origin);
        point0 = point0.transformBy(transformMatrix);
        points.Add(point0);

        // 1
        OdGeVector3d moveVector1 = vAxis - uAxis;
        moveVector1 = moveVector1 * cuttingPlaneSize * OdTvCuttingPlaneDragger.OdTvCuttingplaneSizeCoeff;
        transformMatrix.setToIdentity();
        transformMatrix.setToTranslation(moveVector1);
        OdGePoint3d point1 = new OdGePoint3d(origin);
        point1 = point1.transformBy(transformMatrix);
        points.Add(point1);

        // 2
        OdGeVector3d moveVector2 = vAxis + uAxis;
        moveVector2 = moveVector2 * cuttingPlaneSize * OdTvCuttingPlaneDragger.OdTvCuttingplaneSizeCoeff;
        transformMatrix.setToIdentity();
        transformMatrix.setToTranslation(moveVector2);
        OdGePoint3d point2 = new OdGePoint3d(origin);
        point2 = point2.transformBy(transformMatrix);
        points.Add(point2);

        // 3
        OdGeVector3d moveVector3 = uAxis - vAxis;
        moveVector3 = moveVector3 * cuttingPlaneSize * OdTvCuttingPlaneDragger.OdTvCuttingplaneSizeCoeff;
        transformMatrix.setToIdentity();
        transformMatrix.setToTranslation(moveVector3);
        OdGePoint3d point3 = new OdGePoint3d(origin);
        point3 = point3.transformBy(transformMatrix);
        points.Add(point3);

        OdInt32Array faces = new OdInt32Array();
        faces.Add(4);
        faces.Add(0);
        faces.Add(1);
        faces.Add(2);
        faces.Add(3);

        // append shell to cutting plane entity
        OdTvGeometryDataId cuttingPlaneShellId = pCuttingPlanesEntity.appendShell(points, faces);
        cuttingPlaneShellId.openAsShell().setDisableLighting(true);

        // append boundary polyline to cutting plane entity
        points.Add(point0);
        pCuttingPlanesEntity.appendPolyline(points);

        //notify dragger that cutting plane was added
        if (_dragger is OdTvCuttingPlaneDragger)
        {
            OdTvCuttingPlaneDragger cuttingPlaneDragger = (OdTvCuttingPlaneDragger)_dragger;
            if (cuttingPlaneDragger != null)
                cuttingPlaneDragger.OnCuttingPlaneAdded(cuttingPlanesEntityId);
        }

        _mm.StopTransaction(mtr);
    }

    private double GetMainModelExtentsDistance()
    {
        double maxDistance = 0d;
        MemoryTransaction mtr = _mm.StartTransaction();

        OdTvModel pModel = _tvActiveModelId.openObject();

        // Get extents
        OdGeExtents3d extents = new OdGeExtents3d();
        OdTvResult res = pModel.getExtents(extents);
        if (res != OdTvResult.tvOk)
            return maxDistance;
        // Get max distance between extents
        OdGePoint3d center = extents.center();
        OdGePoint3d minPoint = extents.minPoint();
        OdGePoint3d maxPoint = extents.maxPoint();

        if ((maxPoint.x - minPoint.x < maxPoint.y - minPoint.y) && (maxPoint.x - minPoint.x < maxPoint.z - minPoint.z))
            maxDistance = maxPoint.x - minPoint.x;
        else if ((maxPoint.y - minPoint.y < maxPoint.x - minPoint.x) && (maxPoint.y - minPoint.y < maxPoint.z - minPoint.z))
            maxDistance = maxPoint.y - minPoint.y;
        else if ((maxPoint.z - minPoint.z < maxPoint.x - minPoint.x) && (maxPoint.z - minPoint.z < maxPoint.y - minPoint.y))
            maxDistance = maxPoint.z - minPoint.z;
        else
            maxDistance = maxPoint.x - minPoint.x;

        _mm.StopTransaction(mtr);
        return maxDistance;
    }

    public bool AddCuttingPlane(OdGeVector3d axis, OdTvResult rc)
    {
        MemoryTransaction mtr = _mm.StartTransaction();

        OdTvGsView pActiveView = GetActiveTvExtendedView().getViewId().openObject(OdTv_OpenMode.kForWrite);
        if (pActiveView == null)
        {
            rc = OdTvResult.tvThereIsNoActiveView;
            _mm.StopTransaction(mtr);
            return false;
        }

        uint nPlanes = pActiveView.numCuttingPlanes();
        if (nPlanes >= _odTvCuttingPlaneMaxNum)
        {
            _mm.StopTransaction(mtr);
            return false;
        }

        if (axis.isZeroLength())
        {
            rc = OdTvResult.tvCuttingPlaneZeroNormal;
            _mm.StopTransaction(mtr);
            return false;
        }

        OdGeBoundBlock3d extents = new OdGeBoundBlock3d();
        if (!GetActiveTvExtendedView().getCachedExtents(extents))
        {
            if (pActiveView.viewExtents(extents))
            {
                if (!pActiveView.isPerspective())
                    extents.setToBox(true);

                OdGeMatrix3d xWorldToEye = pActiveView.viewingMatrix();
                OdGeMatrix3d xEyeToWorld = xWorldToEye.invert();
                // transform extents to WCS
                extents.transformBy(xEyeToWorld);
            }
        }

        OdGePoint3d center = extents.center();
        OdGePlane plane = new OdGePlane(center, axis);

        // add cutting plae
        pActiveView.addCuttingPlane(plane);
        //update filling parameters first time
        if (nPlanes == 0)
        {
            pActiveView.setEnableCuttingPlaneFill(SectioningOptions.IsFilled, SectioningOptions.FillingColor);
            pActiveView.setCuttingPlaneFillPatternEnabled(SectioningOptions.FillingPatternEnabled,
              SectioningOptions.FillingPaternStyle, SectioningOptions.FillingPatternColor);
        }

        try
        {
            MemoryTransaction mtrDev = _mm.StartTransaction();
            OdTvGsDevice pDevice = TvGsDeviceId.openObject(OdTv_OpenMode.kForWrite);
            if (SectioningOptions.IsShown)
            {
                // if it is the first added object
                if (nPlanes == 0)
                {
                    //here we should add the view to the device and to the active view
                    pDevice.addView(_cuttingPlanesViewId);
                    //add view as sibling to an active
                    pActiveView.addSibling(_cuttingPlanesViewId);
                }
                //create geometry for the new sectioning plane
                DrawCuttingPlane((int)pActiveView.numCuttingPlanes() - 1, pActiveView, true);
            }

            //pDevice.invalidate();
            //Invalidate();
            UpdateCadView();
            _mm.StopTransaction(mtrDev);
        }
        catch (System.Exception)
        {

        }

        _mm.StopTransaction(mtr);
        return true;
    }

    public void RemoveCuttingPlanes()
    {
        MemoryTransaction mtr = _mm.StartTransaction();
        OdTvGsView pActiveView = GetActiveTvExtendedView().getViewId().openObject(OdTv_OpenMode.kForWrite);
        if (pActiveView == null)
        {
            _mm.StopTransaction(mtr);
            return;
        }

        try
        {
            OdTvGsDevice pDevice = TvGsDeviceId.openObject(OdTv_OpenMode.kForWrite);
            if (SectioningOptions.IsShown)
            {
                //notify dragger
                if (_dragger is OdTvCuttingPlaneDragger)
                {
                    OdTvCuttingPlaneDragger cuttingPlaneDragger = (OdTvCuttingPlaneDragger)_dragger;
                    if (cuttingPlaneDragger != null)
                        cuttingPlaneDragger.OnRemoveCuttingPlanes();
                }

                // remove geometry for the sectioning planes
                if (!_cuttingPlanesModelId.isNull())
                {
                    OdTvModel pMoveModel = _cuttingPlanesModelId.openObject(OdTv_OpenMode.kForWrite);
                    if (pMoveModel != null)
                        pMoveModel.clearEntities();
                }

                // remove view as sibling from an active
                pActiveView.removeSibling(_cuttingPlanesViewId);

                // remove view from device
                pDevice.removeView(_cuttingPlanesViewId);
            }
            //remove cutting planes
            pActiveView.removeCuttingPlanes();
            //pDevice.invalidate();
            //Invalidate();
            UpdateCadView();

        }
        catch (System.Exception)
        {
        }

        _mm.StopTransaction(mtr);
    }

    public void ClearOda()
    {
        ClearDevices();
        ClearDatabases();
        ViewControl?.SetFileLoaded(false, FilePath, (statusText) => ServiceFactory.EventSrv.GetEvent<AppStatusTextChanged>().Publish(statusText));
        _isFileLoading = false;
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

            TvDefaultModelId?.Dispose();
            TvDefaultModelId = null;

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
    #endregion

    public void UpdateHclZoomTransformations(bool isPinchZoom = false)
    {
        if (!_isFileLoading)
        {
            return;
        }
        var scaleFactor = GetScaleFactor();
        _hclStation?.ScaleModelAtLocation(scaleFactor, HclStationLocation);
        _hclPrism?.ScaleModelAtLocation(scaleFactor, HclPrismLocation);
        if (isPinchZoom)
        {
            OptimizePointTransformations();
        }
        else
        {
            _hclPointContainer?.UpdateTransformations(GetPointScaleFactor());
        }
    }

    private void SetupStyles(OdTvDatabase database)
    {
        var tvStyle = new TvStyle(database);

        tvStyle.CreateLineStyle();
        tvStyle.CreateHighlightStyle(HighlightStyle.Red);
        tvStyle.CreateHighlightStyle(HighlightStyle.Point);
        //tvStyle.CreateViewBackgroundStyle();
    }

    internal void SaveFileAsVsfx(string fileName)
    {
        using var vsfxExportOptions = new OdTvVSFExportOptions(OdTvVSFExportOptions_Compression.kInt32,
            OdTvVSFExportOptions_NormalsCompression.kTwoInt32);
        using var tvDatabase = TvDatabaseId.openObject(OdTv_OpenMode.kForWrite);
        tvDatabase.writeVSFX(fileName, vsfxExportOptions);
    }
    internal void ToggleTextTransformation(bool showText)
    {
        _hclPointContainer?.TogglePointText(showText);
        UpdateCadView();
    }
}

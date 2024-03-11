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

namespace HCL_ODA_TestPAD.ViewModels;

public class HclCadImageViewModel : CadImageTabViewModelBase,
    IOdaSectioning
{
    private readonly IServiceFactory _serviceFactory;
    public ICadImageViewControl ViewControl { get; set; }

    #region App Specific Variables
    public MainWindowViewModel VM { get; set; }
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
    private readonly MemoryManager MM = MemoryManager.GetMemoryManager();
    
    // Visualize database
    private OdTvDatabaseId _dbId = null;
    public OdTvDatabaseId TvDatabaseId
    {
        get { return _dbId; }
        set { _dbId = value; }
    }
    public OdTvGsDeviceId TvGsDeviceId { get; set; }

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
    public static int OD_TV_CUTTING_PLANE_MAX_NUM = 5;

    public static OdTvRegAppId AppTvId { get; set; }
    private Dictionary<ulong, OdTvExtendedView> _extendedViewDict = new Dictionary<ulong, OdTvExtendedView>();

    private OdTvAnimation _animation = null;

    // list with selected items(bold nodes in model browser)
    public List<TvTreeItem> SelectedNodes = new List<TvTreeItem>();

    public OdTvSelectionSet SelectionSet = null;
    public TvWpfViewWCS WCS { get; set; }
    public TvDatabaseInfo DatabaseInfo { get; set; }
    private CadModel _cadModel;
    #endregion

    public HclCadImageViewModel(IServiceFactory serviceFactory)
    : base(serviceFactory)
    {
        _serviceFactory = serviceFactory;
    }

    public override Task LoadCadModelViewAsync()
    {
        return Task.CompletedTask;
        //var cadFile = CadImageFilePath;
        //_eventAggregator.GetEvent<CadModelLoadedEvent>().Publish();
        //LoadFile(CadImageFilePath);
    }
    public void InitViewModel()
    {
        ILogger logger = new HPLLogger();
        var cadGenFactory = new CadRegenerator(_serviceFactory);
        _cadModel = new CadModel(() => cadGenFactory);
        _cadImageViewModel = new CadImageViewModel(ViewControl, logger, _cadModel, new CadImageViewBitmapService());

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
            ClearODA();
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
            using var odTvModelId = odTvDatabase.createModel("Tv_Model_Default", OdTvModel_Type.kMain);
            // Create entity
            using var enId = odTvModelId.openObject(OdTv_OpenMode.kForWrite).appendEntity("Entity_Default");
            {
                // Create and setup text style in database
                using var textStyle = odTvDatabase.createTextStyle("kMiddleCenter");
                {
                    using var pTextStyle = textStyle.openObject(OdTv_OpenMode.kForWrite);

                    string typeface = "Algerian";
                    int charset = 0;
                    int family = 34;
                    bool bold = true;
                    bool italic = true;
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
                    textId1.openObject().setColor(new OdTvColorDef(204, 0, 51));
                    using var ptxt = textId1.openAsText();
                    // Set text style for text
                    ptxt.setTextStyle(textStyleDef);
                }

                using var textId2 = pEn.appendText(new OdGePoint3d(0.00, -0.05, 0.0), "HCL");
                {
                    textId2.openObject().setColor(new OdTvColorDef(51, 0, 204));
                    using var ptxt = textId2.openAsText();
                    ptxt.setTextStyle(textStyleDef);
                }

                using var textId3 = pEn.appendText(new OdGePoint3d(-0.025, -0.25, 0.0), "GL_Control");
                {
                    textId3.openObject().setColor(new OdTvColorDef(51, 0, 204));
                    using var ptxt = textId3.openAsText();
                    ptxt.setTextStyle(textStyleDef);
                }

                OdGePoint3d[] pnts = new OdGePoint3d[4];
                double xMin = -0.5, xMax = 0.5, yMin = -0.5, yMax = 0.5;
                pnts[0] = new OdGePoint3d(xMin, yMin, 0.0);
                pnts[1] = new OdGePoint3d(xMin, yMax, 0.0);
                pnts[2] = new OdGePoint3d(xMax, yMax, 0.0);
                pnts[3] = new OdGePoint3d(xMax, yMin, 0.0);

                // Create polygon geometry data
                using var rectId = pEn.appendPolygon(pnts);
                {
                    using var pGeom = rectId.openObject();
                    pGeom.setColor(new OdTvColorDef(128, 0, 255));
                    pGeom.setLineWeight(new OdTvLineWeightDef(7));
                }
                pnts.ForEach(p => p.Dispose());
                
            }

            _serviceFactory.AppSettings = _serviceFactory.AppSettings with { CadFileUnit = SurveyUnits.meters };
            CreateDefaultBitmapDevice(width, height, odTvDatabase, odTvModelId);
           
            ViewControl?.SetFileLoaded(true, FilePath, (statusText) => _serviceFactory.EventSrv.GetEvent<AppStatusTextChanged>().Publish(statusText));
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
            odTvGsDevice.setOption(OdTvGsDevice_Options.kUseSceneGraph, _serviceFactory.AppSettings.UseSceneGraph);
            odTvGsDevice.setOption(OdTvGsDevice_Options.kForcePartialUpdate, _serviceFactory.AppSettings.UseForcePartialUpdate);
            odTvGsDevice.setOption(OdTvGsDevice_Options.kBlocksCache, _serviceFactory.AppSettings.UseBlocksCache);

            ConfigureViewSettings(odTvGsViewId);


            using var rect = new OdTvDCRect(0, 800, 600, 0);
            odTvGsDevice.onSize(rect);
            odTvGsDevice.update();

            //TvActiveViewport = 0;
            //SetFrozenLayersVisible(odTvDatabase);
            TvActiveViewport = 0;

            SetFrozenLayersVisible(odTvDatabase);

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
    public void LoadFile(string filepath)
    {
        bool isIfc = false;
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
        catch (Exception ex)
        {
            MessageBox.Show($"Import failed! : {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(1);
        }

        if (TvDatabaseId != null)
        {
            using var odTvDatabase = TvDatabaseId.openObject(OdTv_OpenMode.kForWrite);
            if (_serviceFactory.AppSettings.EnableImportUnitChange)
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
            odTvGsDevice.setForbidImageHighlight(_serviceFactory.AppSettings.SetForbidImageHighlight);
            odTvGsDevice.setOption(OdTvGsDevice_Options.kForcePartialUpdate, _serviceFactory.AppSettings.UseForcePartialUpdate);
            odTvGsDevice.setOption(OdTvGsDevice_Options.kBlocksCache, _serviceFactory.AppSettings.UseBlocksCache);
            odTvGsDevice.setOption(OdTvGsDevice_Options.kForceOffscreenSceneGraph, _serviceFactory.AppSettings.UseForceOffscreenSceneGraph);
            odTvGsDevice.setOption(OdTvGsDevice_Options.kSceneGraphPurgeGrouppedRenders, _serviceFactory.AppSettings.UseSceneGraphPurgeGroupedRenders);
            odTvGsDevice.setOption(OdTvGsDevice_Options.kUseVisualStyles, _serviceFactory.AppSettings.UseVisualStyles);

            ConfigureViewSettings(activeViewId);

            if (!isIfc)
            {
                odTvGsDevice.setOption(OdTvGsDevice_Options.kUseSceneGraph, _serviceFactory.AppSettings.UseSceneGraph);
            }
            else
            {
                odTvGsDevice.setOption(OdTvGsDevice_Options.kUseSceneGraph, _serviceFactory.AppSettings.IfcUseSceneGraph);
            }

            using var rect = new OdTvDCRect(0, 800, 600, 0);
            odTvGsDevice.onSize(rect);
            //odTvGsDevice.update();

            TvActiveViewport = 0;

            SetFrozenLayersVisible(odTvDatabase);

            FilePath = filepath;

            EmitFileLoadedEvents();

            _isFileLoading = true;

            _cadModel.TvGsDeviceId = TvGsDeviceId;

            Zoom(ZoomType.ZoomExtents);
        }
    }

    private void EmitFileLoadedEvents()
    {
        ViewControl?.SetFileLoaded(true, FilePath, (statusText) => _serviceFactory.EventSrv.GetEvent<AppStatusTextChanged>().Publish(statusText));
        _serviceFactory.EventSrv.GetEvent<CadModelLoadedEvent>().Publish(FilePath);
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
                _serviceFactory.AppSettings = _serviceFactory.AppSettings with { CadFileUnit = modelSurveyUnits };
                return (uint)modelSurveyUnits;
            }
        }
        return (uint)UnitsValue.kUnitsMeters;
    }

    private void SetCadDbUnitAsCadFileUnit(OdTvDatabase odTvDatabase)
    {
        SurveyUnits surveyUnit = _serviceFactory.AppSettings.CadImportUnit;
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
                    odTvModel.setUnits(tvUnits, CADModelConstants.UsFeetCoefValue);
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
        if (!_serviceFactory.AppSettings.SetFrozenLayersVisible) return;

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

        using var color = new OdTvColorDef(CADModelConstants.LightColor.R, CADModelConstants.LightColor.G,
                                     CADModelConstants.LightColor.B);
        viewOpen.setDefaultLightingColor(color);
        viewOpen.enableDefaultLighting(true, OdTvGsView_DefaultLightingType.kTwoLights);
        using var db = viewOpen.getDatabase().openObject(OdTv_OpenMode.kForWrite);
        using var background = db.createBackground("Gradient", OdTvGsViewBackgroundId_BackgroundTypes.kGradient);

        using var bg = background.openAsGradientBackground(OdTv_OpenMode.kForWrite);
        if (bg != null)
        {
            bg.setColorTop(new OdTvColorDef(CADModelConstants.GradientColorTop.R, CADModelConstants.GradientColorTop.G, CADModelConstants.GradientColorTop.B));
            bg.setColorMiddle(new OdTvColorDef(CADModelConstants.GradientColorMiddle.R, CADModelConstants.GradientColorMiddle.G, CADModelConstants.GradientColorMiddle.B));
            bg.setColorBottom(new OdTvColorDef(CADModelConstants.GradientColorBottom.R, CADModelConstants.GradientColorBottom.G, CADModelConstants.GradientColorBottom.B));
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
                dwgPmtrs.setObjectNaming(_serviceFactory.AppSettings.DwgSetObjectNaming);
                dwgPmtrs.setStoreSourceObjects(_serviceFactory.AppSettings.DwgSetStoreSourceObjects);
                dwgPmtrs.setFeedbackForChooseCallback(null);
                dwgPmtrs.setImportFrozenLayers(_serviceFactory.AppSettings.DwgSetImportFrozenLayers);
                dwgPmtrs.setClearEmptyObjects(_serviceFactory.AppSettings.DwgSetClearEmptyObjects);
                dwgPmtrs.setNeedCDATree(_serviceFactory.AppSettings.DwgSetNeedCDATree);
                dwgPmtrs.setNeedCollectPropertiesInCDA(_serviceFactory.AppSettings.DwgSetNeedCollectPropertiesInCDA);
            }
            else if (ext == ".ifc")
            {
                var tvIfcImportParams = new OdTvIfcImportParams();
                tvIfcImportParams.setNeedCDATree(_serviceFactory.AppSettings.IfcSetNeedCDATree);
                tvIfcImportParams.setNeedCollectPropertiesInCDA(_serviceFactory.AppSettings.IfcSetNeedCollectPropertiesInCDA);
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
        MemoryTransaction mtr = MM.StartTransaction();
        OdTvDatabase pDb = TvDatabaseId.openObject(OdTv_OpenMode.kForWrite);
        _tvDraggersModelId = pDb.createModel("Draggers", OdTvModel_Type.kDirect, false);
        OdTvSelectDragger selectDragger = new OdTvSelectDragger(this, _tvActiveModelId, TvGsDeviceId, _tvDraggersModelId);
        _dragger = selectDragger;
        DraggerResult res = _dragger.Start(null, TvActiveViewport, null, WCS);
        ActionAfterDragger(res);

        // find service models

        // set projection button
        OdTvGsView view = TvGsDeviceId.openObject().viewAt(TvActiveViewport).openObject();

        MM.StopTransaction(mtr);
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
        if (_serviceFactory.AppSettings.Interactivity)
        {
            //start Interactivity
            using var odTvGsView = TvGsDeviceId.openObject().viewAt(TvActiveViewport).openObject(OdTv_OpenMode.kForWrite);
            odTvGsView.beginInteractivity(_serviceFactory.AppSettings.InteractiveFPS);
        }
    }

    public void MouseMove(System.Windows.Input.MouseEventArgs e, Point position)
    {
        if (_dragger != null)
        {
            if (_mouseDown == MouseButtonState.Pressed)
            {
                DraggerResult res = _dragger.Drag((int)position.X, (int)position.Y);
                //Debug.WriteLine("4-AtvViewer-MouseMoveEvent");
                ActionAfterDragger(res);
                //UpdateCadView();
            }
        }
    }

    public void MouseUp(MouseButtonEventArgs e)
    {
        _mouseDown = MouseButtonState.Released;
        if (_serviceFactory.AppSettings.Interactivity)
        {
            //start Interactivity
            using var odTvGsView = TvGsDeviceId.openObject().viewAt(TvActiveViewport).openObject(OdTv_OpenMode.kForWrite);
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
            UpdateCadView(false);
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
        //check existance of the animation
        //SetAnimation(exView.getAnimation());
        //DisableMarkups();
        //Invalidate();
        UpdateCadView();

        UpdateWCSView();
        //MM.StopTransaction(mtr);
        if (_serviceFactory.AppSettings.ShowCube)
        {
            OnOffViewCube(true);
        }
    }

    private void UpdateWCSView()
    {
        using var parentView = WCS.GetParentView(OdTv_OpenMode.kForRead);
        var parentViewPosition = parentView.position();
        var targetTranslation = OdGeMatrix3d.translation(-parentView.target().asVector());

        using var wcsView = WCS.GetWcsView(OdTv_OpenMode.kForWrite);

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
        MemoryTransaction mtr = MM.StartTransaction();

        OdTvGsView pView = GetActiveTvExtendedView().getViewId().openObject();
        if (pView != null)
            eyeDir = pView.position() - pView.target();

        MM.StopTransaction(mtr);
        return eyeDir;
    }
    public void SetRenderMode(OdTvGsView_RenderMode renderMode)
    {
        //MemoryTransaction mtr = MM.StartTransaction();
        if (TvGsDeviceId != null && !TvGsDeviceId.isNull())
        {
            using var view = TvGsDeviceId.openObject().viewAt(TvActiveViewport).openObject(OdTv_OpenMode.kForWrite);
            OdTvGsView_RenderMode oldMode = view.mode();
            if (oldMode != renderMode)
            {
                view.setMode(renderMode);

                // set mode for WCS
                //if (WCS != null && _serviceFactory.AppSettings.ShowWCS && WCS.IsNeedUpdateWCS(oldMode, renderMode))
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
        MemoryTransaction mtr = MM.StartTransaction();
        if (TvGsDeviceId != null && !TvGsDeviceId.isNull())
        {
            OdTvGsView view = TvGsDeviceId.openObject().viewAt(TvActiveViewport).openObject(OdTv_OpenMode.kForWrite);
            view.setView(view.position(), view.target(), view.upVector(), view.fieldWidth(), view.fieldHeight(), projection);
            //TvDeviceId.openObject().update();
            UpdateCadView();
        }
        MM.StopTransaction(mtr);
    }

    public void SetBackgroundColor(Color color)
    {
        MemoryTransaction mtr = MM.StartTransaction();
        uint iColor = ((uint)(color.R | color.G << 8 | ((color.B) << 16)));
        if (TvGsDeviceId != null && !TvGsDeviceId.isNull())
        {
            OdTvGsDevice dev = TvGsDeviceId.openObject(OdTv_OpenMode.kForWrite);
            dev.setBackgroundColor(iColor);
            dev.update();
        }
        MM.StopTransaction(mtr);
    }
    #endregion

    #region Regen
    public void Regen(OdTvGsDevice_RegenMode rm)
    {
        if (TvGsDeviceId == null)
            return;
        MemoryTransaction mtr = MM.StartTransaction();
        OdTvGsDevice dev = TvGsDeviceId.openObject();
        dev.regen(rm);
        dev.invalidate();
        //dev.update();
        UpdateCadView();
        MM.StopTransaction(mtr);
    }

    public void Regen()
    {
        if (TvGsDeviceId == null)
            return;
        MemoryTransaction mtr = MM.StartTransaction();
        OdTvGsDevice dev = TvGsDeviceId.openObject();
        if (TvActiveViewport > 0)
            dev.viewAt(TvActiveViewport).openObject().regen();
        dev.invalidate();
        //dev.update();
        UpdateCadView();
        MM.StopTransaction(mtr);
    }


    #endregion

    #region Draggers
    private void StartDragger(ODA.Draggers.OdTvDragger dragger, bool useCurrentAsPrevious = false)
    {
        DraggerResult res = DraggerResult.NothingToDo;

        if (_dragger == null)
        {
            res = dragger.Start(null, TvActiveViewport, null, WCS);
        }
        else
        {
            ODA.Draggers.OdTvDragger pPrevDragger = _dragger;
            if (_dragger.HasPrevious())
            {
                DraggerResult res_prev;
                if (useCurrentAsPrevious)
                    _dragger.Finish(out res_prev);
                else
                    pPrevDragger = _dragger.Finish(out res_prev);
                //ActionAfterDragger(res_prev);
            }
            res = dragger.Start(pPrevDragger, TvActiveViewport, null, WCS);
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
            MemoryTransaction mtr = MM.StartTransaction();

            OdTvModel pMarkupModel = _tvMarkupModelId.openObject(OdTv_OpenMode.kForWrite);
            if (pMarkupModel == null)
            {
                MM.StopTransaction(mtr);
                return;
            }
            OdTvEntitiesIterator pIt = pMarkupModel.getEntitiesIterator();
            if (pIt != null && !pIt.done())
            {
                while (!pIt.done())
                {
                    MemoryTransaction mtr2 = MM.StartTransaction();

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

                    MM.StopTransaction(mtr2);
                    pIt.step();
                }
            }

            MM.StopTransaction(mtr);
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
            MemoryTransaction mtr = MM.StartTransaction();
            OdTvDatabase db = _dbId.openObject(OdTv_OpenMode.kForWrite);
            OdTvResult rc = db.writeFile(filePath);
            MM.StopTransaction(mtr);
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
        MemoryTransaction mtr = MM.StartTransaction();
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
        MM.StopTransaction(mtr);
    }

    private OdTvGsDeviceId CreateNewDevice()
    {
        MemoryTransaction mtr = MM.StartTransaction();
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

        MM.StopTransaction(mtr);
        return newDevId;
    }

    public void ExportToPdf(string fileName, bool is2D = true)
    {
        MemoryTransaction mtr = MM.StartTransaction();

        OdTvDatabase db = _dbId.openObject(OdTv_OpenMode.kForWrite);
        if (db == null)
        {
            MessageBox.Show("There is no database for the save!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            MM.StopTransaction(mtr);
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
                MM.StopTransaction(mtr);
                //this.Cursor = Cursors.Arrow;
                return;
            }
        }
        catch
        {
            MessageBox.Show("Export of file " + fileName + " was failed!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            MM.StopTransaction(mtr);
            //this.Cursor = Cursors.Arrow;
            return;
        }

        //this.Cursor = Cursors.Arrow;
        MM.StopTransaction(mtr);
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
            MemoryTransaction mtr = MM.StartTransaction();
            _tvMarkupModelId = _dbId.openObject(OdTv_OpenMode.kForWrite).createModel(OdTvMarkupDragger.NameOfMarkupModel, OdTvModel_Type.kDirect, true);
            MM.StopTransaction(mtr);
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
        MemoryTransaction mtr = MM.StartTransaction();

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

        MM.StopTransaction(mtr);
    }

    public void LoadMarkup()
    {
        MemoryTransaction mtr = MM.StartTransaction();

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

        MM.StopTransaction(mtr);
    }

    #endregion

    #region Appearance commands

    public void OnOffWCS(bool bEnable)
    {
        if (TvGsDeviceId == null)
            return;

        if (bEnable)
        {
            if (WCS == null)
                CreateWCS();
            else
                WCS.UpdateWCS();
        }
        else if (!bEnable && WCS != null)
        {
            WCS.removeWCS();
        }
        //MemoryTransaction mtr = MM.StartTransaction();
        //TvDeviceId.openObject().update();
        //Invalidate();
        UpdateCadView();
        //MM.StopTransaction(mtr);
    }

    private void CreateWCS()
    {
        if (WCS != null)
            WCS.removeWCS();

        MemoryTransaction mtr = MM.StartTransaction();
        WCS = new TvWpfViewWCS(TvDatabaseId, TvGsDeviceId.openObject().viewAt(TvActiveViewport));
        OdTvGsView pWcsView = WCS.GetWcsViewId().openObject(OdTv_OpenMode.kForWrite);
        OdTvGsView activeView = WCS.GetParentViewId().openObject();
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
        WCS.UpdateWCS();

        MM.StopTransaction(mtr);
    }

    public void OnOffViewCube(bool bEnable)
    {
        if (TvGsDeviceId == null)
            return;
        MemoryTransaction mtr = MM.StartTransaction();
        OdTvExtendedView extView = GetActiveTvExtendedView();
        if (extView != null && extView.getEnabledViewCube() != bEnable)
        {
            extView.setEnabledViewCube(bEnable);
            //Invalidate();
            UpdateCadView();
        }
        MM.StopTransaction(mtr);
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
        MemoryTransaction mtr = MM.StartTransaction();
        OdTvExtendedView exView = GetActiveTvExtendedView();
        if (exView != null)
            exView.setAnimationEnabled(bEnable);
        MM.StopTransaction(mtr);
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

        MemoryTransaction mtr = MM.StartTransaction();

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

        MM.StopTransaction(mtr);
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

        MemoryTransaction mtr = MM.StartTransaction();
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

        MM.StopTransaction(mtr);

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
        MemoryTransaction mtr = MM.StartTransaction();

        OdTvGsView pView = GetActiveTvExtendedView().getViewId().openObject(OdTv_OpenMode.kForWrite);
        if (pView == null)
        {
            MM.StopTransaction(mtr);
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

        MM.StopTransaction(mtr);
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
        MemoryTransaction mtr = MM.StartTransaction();
        SectioningOptions.IsShown = !SectioningOptions.IsShown;

        bool bRet = false;

        OdTvGsView pActiveView = GetActiveTvExtendedView().getViewId().openObject(OdTv_OpenMode.kForWrite);
        OdTvGsDevice pDevice = TvGsDeviceId.openObject(OdTv_OpenMode.kForWrite);
        if (pActiveView == null || pDevice == null)
        {
            MM.StopTransaction(mtr);
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
        MM.StopTransaction(mtr);
        return bRet;
    }

    protected void DrawCuttingPlane(int index, OdTvGsView pView, bool bNeedNotifyDragger = false)
    {
        if (pView == null)
            return;
        MemoryTransaction mtr = MM.StartTransaction();

        OdGePlane cuttingPlane = new OdGePlane();
        OdTvResult rc = pView.getCuttingPlane((uint)index, cuttingPlane);
        if (rc != OdTvResult.tvOk)
        {
            MM.StopTransaction(mtr);
            return;
        }

        OdTvModel pCuttingPlaneModel = _cuttingPlanesModelId.openObject(OdTv_OpenMode.kForWrite);
        // create cutting plane entity
        OdTvEntityId cuttingPlanesEntityId = pCuttingPlaneModel.appendEntity("$_CUTTINGPLANE_ENTITY" + index);
        //set a few parameters to the cutting plane
        OdTvEntity pCuttingPlanesEntity = cuttingPlanesEntityId.openObject(OdTv_OpenMode.kForWrite);
        pCuttingPlanesEntity.setColor(new OdTvColorDef(175, 175, 175));
        pCuttingPlanesEntity.setLineWeight(new OdTvLineWeightDef(OdTvCuttingPlaneDragger.OD_TV_CUTTINGPLANE_EDGE_DEFAULT_LINEWEIGHT));
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
        double cuttingPlaneSize = getMainModelExtentsDistance() / 2d;

        OdGePoint3dVector points = new OdGePoint3dVector();

        OdGeMatrix3d transformMatrix = new OdGeMatrix3d();
        // 0
        OdGeVector3d moveVector0 = -vAxis - uAxis;
        moveVector0 = moveVector0 * cuttingPlaneSize * OdTvCuttingPlaneDragger.OD_TV_CUTTINGPLANE_SIZE_COEFF;
        transformMatrix.setToIdentity();
        transformMatrix.setToTranslation(moveVector0);
        OdGePoint3d point0 = new OdGePoint3d(origin);
        point0 = point0.transformBy(transformMatrix);
        points.Add(point0);

        // 1
        OdGeVector3d moveVector1 = vAxis - uAxis;
        moveVector1 = moveVector1 * cuttingPlaneSize * OdTvCuttingPlaneDragger.OD_TV_CUTTINGPLANE_SIZE_COEFF;
        transformMatrix.setToIdentity();
        transformMatrix.setToTranslation(moveVector1);
        OdGePoint3d point1 = new OdGePoint3d(origin);
        point1 = point1.transformBy(transformMatrix);
        points.Add(point1);

        // 2
        OdGeVector3d moveVector2 = vAxis + uAxis;
        moveVector2 = moveVector2 * cuttingPlaneSize * OdTvCuttingPlaneDragger.OD_TV_CUTTINGPLANE_SIZE_COEFF;
        transformMatrix.setToIdentity();
        transformMatrix.setToTranslation(moveVector2);
        OdGePoint3d point2 = new OdGePoint3d(origin);
        point2 = point2.transformBy(transformMatrix);
        points.Add(point2);

        // 3
        OdGeVector3d moveVector3 = uAxis - vAxis;
        moveVector3 = moveVector3 * cuttingPlaneSize * OdTvCuttingPlaneDragger.OD_TV_CUTTINGPLANE_SIZE_COEFF;
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

        MM.StopTransaction(mtr);
    }

    private double getMainModelExtentsDistance()
    {
        double maxDistance = 0d;
        MemoryTransaction mtr = MM.StartTransaction();

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

        MM.StopTransaction(mtr);
        return maxDistance;
    }

    public bool AddCuttingPlane(OdGeVector3d axis, OdTvResult rc)
    {
        MemoryTransaction mtr = MM.StartTransaction();

        OdTvGsView pActiveView = GetActiveTvExtendedView().getViewId().openObject(OdTv_OpenMode.kForWrite);
        if (pActiveView == null)
        {
            rc = OdTvResult.tvThereIsNoActiveView;
            MM.StopTransaction(mtr);
            return false;
        }

        uint nPlanes = pActiveView.numCuttingPlanes();
        if (nPlanes >= OD_TV_CUTTING_PLANE_MAX_NUM)
        {
            MM.StopTransaction(mtr);
            return false;
        }

        if (axis.isZeroLength())
        {
            rc = OdTvResult.tvCuttingPlaneZeroNormal;
            MM.StopTransaction(mtr);
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
            MemoryTransaction mtrDev = MM.StartTransaction();
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
            MM.StopTransaction(mtrDev);
        }
        catch (System.Exception)
        {

        }

        MM.StopTransaction(mtr);
        return true;
    }

    public void RemoveCuttingPlanes()
    {
        MemoryTransaction mtr = MM.StartTransaction();
        OdTvGsView pActiveView = GetActiveTvExtendedView().getViewId().openObject(OdTv_OpenMode.kForWrite);
        if (pActiveView == null)
        {
            MM.StopTransaction(mtr);
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

        MM.StopTransaction(mtr);
    }

    public void ClearODA()
    {
        ClearDevices();
        ClearDatabases();
        ViewControl?.SetFileLoaded(false, FilePath, (statusText) => _serviceFactory.EventSrv.GetEvent<AppStatusTextChanged>().Publish(statusText));
    }
    private void ClearDevices()
    {
        _cadImageViewModel.OnVisibilityChanged(false);

        MemoryTransaction mtr = MM.StartTransaction();

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
        MM.StopTransaction(mtr);

        _cadModel.Dispose();
        Debug.WriteLine($"ClearDevices : {FilePath}");
    }
    private void ClearDatabases()
    {
        OdTvFactoryId factId = TV_Visualize_Globals.odTvGetFactory();
        factId.clearDatabases();
        Debug.WriteLine($"ClearDatabases : {FilePath}");
    }

    public override void CloseTabView()
    {
        ClearODA();
    }

    public void ShowFPS()
    {
        OnOffFPS(_serviceFactory.AppSettings.ShowFPS);
    }
    public void ShowWCS()
    {
        OnOffWCS(_serviceFactory.AppSettings.ShowWCS);
    }
    public void ShowCube()
    {
        OnOffViewCube(_serviceFactory.AppSettings.ShowCube);
    }
    public void ShowCustomModels()
    {
        ShowFPS();
        ShowWCS();
    }
    #endregion
}

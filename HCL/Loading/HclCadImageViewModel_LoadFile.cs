using ODA.Visualize.TV_Visualize;
using ODA.Visualize.TV_VisualizeTools;
using System.Threading.Tasks;
using System.Threading;
using HCL_ODA_TestPAD.Mvvm.Events;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security;
using System;
using System.Windows.Forms;
using HCL_ODA_TestPAD.HCL.CadUnits;
using HCL_ODA_TestPAD.HCL;
using ODA.Kernel.TD_RootIntegrated;
using System.Linq;
using HCL_ODA_TestPAD.HCL.Visualize;

namespace HCL_ODA_TestPAD.ViewModels;

public partial class HclCadImageViewModel
{
    #region Unit Variables
    private UnitsValue CadUnits { get; set; }
    #endregion

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
            //Off-screen vectorization devices disable partial update mode.
            //This property can be used to turn on partial update mode in this case.
            //Forcing partial updating is useful for tests, and in the case of multi-threaded vectorization it is necessary for off-screen rendering.
            odTvGsDevice.setOption(OdTvGsDevice_Options.kForcePartialUpdate, ServiceFactory.AppSettings.UseForcePartialUpdate);
            //Enables caching of a hierarchical block structure.
            odTvGsDevice.setOption(OdTvGsDevice_Options.kBlocksCache, ServiceFactory.AppSettings.UseBlocksCache);
            //Improves when true Bitmap Device SceneGraph Rendering performance when using off-screen rendering.
            odTvGsDevice.setOption(OdTvGsDevice_Options.kForceOffscreenSceneGraph, ServiceFactory.AppSettings.UseForceOffscreenSceneGraph);
            //Improves when false Bitmap Device SceneGraph Rendering performance when using off-screen rendering.
            odTvGsDevice.setOption(OdTvGsDevice_Options.kSceneGraphPurgeGrouppedRenders, ServiceFactory.AppSettings.UseSceneGraphPurgeGroupedRenders);
            //If Highlighting works properly, set this parameter to OFF to increase shading mode render performance
            //Specifically for IFC Files, but let's use it for all supporting file types, i.e, .dwg, dxf, .ifc
            odTvGsDevice.setOption(OdTvGsDevice_Options.kUseVisualStyles, ServiceFactory.AppSettings.UseVisualStyles);
            //The sub-entity highlighting mode enables the ability to cache geometry markers for entity geometry parts.
            //This option significantly speeds up the highlighting of geometry or sub-geometry.
            odTvGsDevice.setOption(OdTvGsDevice_Options.kUseDynamicSubEntHlt, ServiceFactory.AppSettings.UseDynamicSubEntHighlighting);
            SetInteractivityOptions(odTvGsDevice);
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
            UseHighlightStyle(odTvDatabase);
            FilePath = filepath;

            EmitFileLoadedEvents();
            SetSelectionLevel();
            _isFileLoading = true;
            _isIfc = taskIsIfc;
            _cadModel.TvGsDeviceId = TvGsDeviceId;

            //Zoom(ZoomType.ZoomExtents);
            Set3DView(OdTvExtendedView_e3DViewType.kTop);
            Regen(OdTvGsDevice_RegenMode.kRegenAll);
            UpdateCadView(true);
        }
    }

    private void SetInteractivityOptions(OdTvGsDevice device)
    {
        using var interactOptions = new OdTvGsDevice.InteractivityOptions();
        interactOptions.setDisplayPostEffects(ServiceFactory.AppSettings.UseDisplayPostEffects);
        device.setInteractivityOptions(interactOptions);
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
                dwgPmtrs.setFeedbackForChooseCallback(PCallback);
                dwgPmtrs.setImportFrozenLayers(ServiceFactory.AppSettings.DwgSetImportFrozenLayers);
                dwgPmtrs.setClearEmptyObjects(ServiceFactory.AppSettings.DwgSetClearEmptyObjects);
                dwgPmtrs.setNeedCDATree(ServiceFactory.AppSettings.DwgSetNeedCdaTree);
                dwgPmtrs.setPlineSimplification(ServiceFactory.AppSettings.DwgLineSimplification);
                dwgPmtrs.setNeedCollectPropertiesInCDA(ServiceFactory.AppSettings.DwgSetNeedCollectPropertiesInCda);
            }
            else if (ext == ".ifc")
            {
                var tvIfcImportParams = new OdTvIfcImportParams();
                tvIfcImportParams.setNeedCDATree(ServiceFactory.AppSettings.IfcSetNeedCdaTree);
                tvIfcImportParams.setNeedCollectPropertiesInCDA(ServiceFactory.AppSettings.IfcSetNeedCollectPropertiesInCda);
                tvIfcImportParams.setFeedbackForChooseCallback(PCallbackForIfc);
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
    private static void PCallbackForIfc(OdTvFilerFeedbackForChooseObject argument)
    {

    }
    private void SetupStyles(OdTvDatabase database)
    {
        var tvStyle = new TvStyle(database);

        tvStyle.CreateLineStyle();
        var highLightColor = ServiceFactory.AppSettings.HighlightColor;
        var faceLightColor = ServiceFactory.AppSettings.FaceHighlightColor;
        tvStyle.CreateHighlightStyle(HighlightStyle.EntityGeometry, highLightColor, faceLightColor);
        //tvStyle.CreateHighlightStyle(HighlightStyle.Point);
        //tvStyle.CreateViewBackgroundStyle();
    }
    private void UseHighlightStyle(OdTvDatabase database)
    {
        var tvStyle = new TvStyle(database);
        tvStyle.SetHighlightStyle(TvGsDeviceId, HighlightStyle.EntityGeometry.ToString());
    }

    internal void SaveFileAsVsfx(string fileName)
    {
        using var vsfxExportOptions = new OdTvVSFExportOptions(OdTvVSFExportOptions_Compression.kInt32,
            OdTvVSFExportOptions_NormalsCompression.kTwoInt32);
        using var tvDatabase = TvDatabaseId.openObject(OdTv_OpenMode.kForWrite);
        tvDatabase.writeVSFX(fileName, vsfxExportOptions);
    }
}
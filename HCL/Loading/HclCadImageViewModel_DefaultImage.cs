using ODA.Visualize.TV_Visualize;
using HCL_ODA_TestPAD.ViewModels.Base;
using System;
using System.Windows.Forms;
using HCL_ODA_TestPAD.Functional.Extensions;
using HCL_ODA_TestPAD.HCL.CadUnits;
using HCL_ODA_TestPAD.Mvvm.Events;
using ODA.Kernel.TD_RootIntegrated;
using HCL_ODA_TestPAD.Performance;

namespace HCL_ODA_TestPAD.ViewModels;

public partial class HclCadImageViewModel
{
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

            //SetupStyles(odTvDatabase);

            FilePath = "HILTI-HCL-GL_Control";

            EmitFileLoadedEvents();

            SetSelectionLevel();

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
    public void GenerateDefaultCadImage(int width, int height)
    {
        using var odTvFactoryId = TV_Visualize_Globals.odTvGetFactory();
        TvDatabaseId = odTvFactoryId.createDatabase();
        try
        {
            OdTvResult rc = OdTvResult.tvCannotOpenFile;
            using var odTvDatabase = TvDatabaseId.openObject(OdTv_OpenMode.kForWrite, ref rc);
            // Create model
            TvDefaultModelId = odTvDatabase.createModel("Tv_Model_Default", OdTvModel_Type.kMain);
            // Create entity
            using var odModelObj = TvDefaultModelId.openObject(OdTv_OpenMode.kForWrite);
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
            CreateDefaultBitmapDevice(width, height, odTvDatabase, TvDefaultModelId);

            ViewControl?.SetFileLoaded(true, FilePath, (statusText) => ServiceFactory.EventSrv.GetEvent<AppStatusTextChanged>().Publish(statusText));
        }
        catch
        {
            MessageBox.Show("Cannot create new file!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }


}
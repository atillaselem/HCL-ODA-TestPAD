using HCL_ODA_TestPAD.Dialogs;
using HCL_ODA_TestPAD.ODA.Draggers.Construct;
using HCL_ODA_TestPAD.ODA.Draggers.Markups;
using HCL_ODA_TestPAD.ODA.Draggers;
using HCL_ODA_TestPAD.ODA.ModelBrowser;
using HCL_ODA_TestPAD.ODA.WCS;
using HCL_ODA_TestPAD.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using MessageBox = System.Windows.Forms.MessageBox;
using System.Windows.Forms;
using ODA.Kernel.TD_RootIntegrated;
using ODA.Visualize.TV_Visualize;
using ODA.Visualize.TV_VisualizeTools;

namespace HCL_ODA_TestPAD.ViewModels;

public partial class HclCadImageViewModel : CadImageTabViewModelBase, IOdaSectioning
{
    private readonly MemoryManager _mm = MemoryManager.GetMemoryManager();

    #region Image Bufferring Variables
    static int _widthResized, _heightResized;
    #endregion

    #region ODA Visualize Variables
    // Visualize database
    public OdTvGeometryDataId TvPolygonGeometryId { get; set; }
    // current active dragger
    private ODA.Draggers.OdTvDragger _dragger = null;
    // index of active view
    private OdTvModelId _tvActiveModelId = null;
    private OdTvModelId _tvMarkupModelId = null;
    private OdTvModelId _tvDraggersModelId = null;
    public ulong InnerRectangleEntityHandle { get; internal set; }
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
    public TvWpfViewWcs Wcs { get; set; }
    public TvDatabaseInfo DatabaseInfo { get; set; }
    #endregion

    #region Create GL_Device
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

    #region Animation
    public void SetAnimation(OdTvAnimation animation)
    {
        _animation = animation;
        if (_animation != null)
            _animation.start();
        else if (_dragger != null)
            _dragger.NotifyAboutViewChange(DraggerViewChangeType.ViewChangeFull);
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

    #endregion
}

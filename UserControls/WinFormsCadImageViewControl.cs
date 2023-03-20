using HCL_ODA_TestPAD.Dialogs;
using HCL_ODA_TestPAD.HCL;
using HCL_ODA_TestPAD.HCL.CadUnits;
using HCL_ODA_TestPAD.ODA.Draggers;
using HCL_ODA_TestPAD.ODA.Draggers.Construct;
using HCL_ODA_TestPAD.ODA.Draggers.Markups;
using HCL_ODA_TestPAD.ODA.Draggers.Navigation;
using HCL_ODA_TestPAD.ODA.ModelBrowser;
using HCL_ODA_TestPAD.ODA.WCS;
using HCL_ODA_TestPAD.Services;
using HCL_ODA_TestPAD.Settings;
using HCL_ODA_TestPAD.ViewModels;
using HCL_ODA_TestPAD.ViewModels.Base;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Teigha.Core;
using Teigha.Visualize;
using static Teigha.Visualize.OdTvPdfExportParams;

namespace HCL_ODA_TestPAD.UserControls
{
    public partial class WinFormsCadImageViewControl : UserControl, IOdaSectioning, IOpenGLES2Control
    {
        private readonly IServiceFactory _serviceFactory;
        public WinFormsCadImageViewControl Adapter => this;
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
        private OdTvGsDeviceId _tvDeviceId = null;
        public OdTvGsDeviceId TvGsDeviceId
        {
            get { return _tvDeviceId; }
            set { _tvDeviceId = value; }
        }

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
        private int CuttingPlaneNum = 0;
        public OdTvSectioningOptions SectioningOptions { get; set; }
        public static int OD_TV_CUTTING_PLANE_MAX_NUM = 5;

        public static OdTvRegAppId AppTvId { get; set; }
        private Dictionary<ulong, OdTvExtendedView> _extendedViewDict = new Dictionary<ulong, OdTvExtendedView>();

        private OdTvAnimation _animation = null;

        // list with selected items(bold nodes in model browser)
        public List<TvTreeItem> SelectedNodes = new List<TvTreeItem>();

        public OdTvSelectionSet SelectionSet = null;
        public TvWpfViewWCS WCS { get; set; }
        public TvDatabaseInfo DatabaseInfo { get; set; }
        // flag for shift button pressed
        bool _isShiftPressed = false;
        // index of wcs viewport
        public int TvWcsViewportInd = -1;
        enum MouseDownState
        {
            None,
            LeftMouseBtn,
            MiddleMouseBtn
        }

        private MouseDownState _mouseDown = MouseDownState.None;
        #endregion

        private Func<CadRegenerator> _cadRegenFactory;
        public WinFormsCadImageViewControl(
        MainWindowViewModel vm,
        IServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
            InitializeComponent();
            this.Paint += OdTvWpfView_Paint; ;
            this.Resize += ResizePanel;
            this.MouseWheel += MouseWheelEvent;
            this.MouseDown += MouseDownEvent;
            this.MouseUp += MouseUpEvent;
            this.MouseMove += MouseMoveEvent;
            this.KeyPress += TextInputEvent;
            VM = vm;
            Size = new Size((int)VM.AppMainWindow.Width, (int)VM.AppMainWindow.Height);
            SectioningOptions = new ODA.Draggers.OdTvSectioningOptions();

            var cadGenerator = new CadRegenerator(serviceFactory);
            _cadRegenFactory = () => cadGenerator;
        }

        public void ClearDevices()
        {
            MemoryTransaction mtr = MM.StartTransaction();

            foreach (var extView in _extendedViewDict)
            {
                extView.Value.Dispose();
            }

            _extendedViewDict.Clear();

            if (_dbId != null && !_dbId.isNull() && _dbId.isValid())
                _dbId.openObject(OpenMode.kForWrite).clearDevices();

            MM.StopTransaction(mtr);
        }

        #region Paint and initialization

        private void OdTvWpfView_Paint(object sender, PaintEventArgs e)
        {
            if (!this.Disposing && _tvDeviceId != null && !_tvDeviceId.isNull())
            {
                MemoryTransaction mtr = MM.StartTransaction();

                OdTvGsDevice pDevice = _tvDeviceId.openObject();
                pDevice.TryAutoRegeneration(_cadRegenFactory).update();

                if (_animation != null && _animation.isRunning())
                {
                    _animation.step();
                    Invalidate();
                    if (_dragger != null && !_animation.isRunning())
                        _dragger.NotifyAboutViewChange(DraggerViewChangeType.ViewChangeFull);
                }

                MM.StopTransaction(mtr);
            }
        }

        private void ResizePanel(object sender, EventArgs e)
        {
            if (_tvDeviceId != null && !this.Disposing)
            {
                MemoryTransaction mtr = MM.StartTransaction();
                OdTvGsDevice dev = _tvDeviceId.openObject(OpenMode.kForWrite);
                if (this.Width > 0 && this.Height > 0)
                {
                    dev.onSize(new OdTvDCRect(0, Width, Height, 0));
                    dev.TryAutoRegeneration(_cadRegenFactory).update();
                }
                MM.StopTransaction(mtr);
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e) { }

        private void Init()
        {
            if (_dbId == null || _tvDeviceId == null)
                return;
            MemoryTransaction mtr = MM.StartTransaction();
            OdTvDatabase pDb = _dbId.openObject(OpenMode.kForWrite);
            _tvDraggersModelId = pDb.createModel("Draggers", OdTvModel.Type.kDirect, false);
            OdTvSelectDragger selectDragger = new OdTvSelectDragger(this, _tvActiveModelId, _tvDeviceId, _tvDraggersModelId);
            _dragger = selectDragger;
            DraggerResult res = _dragger.Start(null, TvActiveViewport, Cursor, WCS);
            ActionAfterDragger(res);
            bool exist;
            AppTvId = _dbId.openObject().registerAppName("WPF Visualize Viewer", out exist);

            // find service models
            OdTvResult rc = new OdTvResult();
            rc = OdTvResult.tvOk;
            _tvMarkupModelId = _dbId.openObject().findModel(OdTvMarkupDragger.NameOfMarkupModel, ref rc);
            if (rc != OdTvResult.tvOk)
                _tvMarkupModelId = null;

            // cutting planes
            _cuttingPlanesModelId = pDb.createModel("$ODA_TVVIEWER_SECTIONING_MODEL_" + CuttingPlaneNum, OdTvModel.Type.kMain, false);
            _cuttingPlanesViewId = TvGsDeviceId.openObject(OpenMode.kForWrite).createView("$ODA_TVVIEWER_SECTIONING_VIEW_" + CuttingPlaneNum++);
            OdTvGsView pVsectioningView = _cuttingPlanesViewId.openObject(OpenMode.kForWrite);
            pVsectioningView.addModel(_cuttingPlanesModelId);
            pVsectioningView.setMode(OdTvGsView.RenderMode.kGouraudShaded);

            // set projection button
            OdTvGsView view = _tvDeviceId.openObject().viewAt(TvActiveViewport).openObject();
            if (view.isPerspective())
                VM.AppMainWindow.PerspectiveBtn.IsChecked = true;
            else
                VM.AppMainWindow.IsometricBtn.IsChecked = true;
            // set render mode
            VM.SetRenderModeButton(view.mode());

            //VM.AppMainWindow.PropertiesPalette.InitializePalette(_tvDevice, this);

            //// enable or disable wcs, fps and grid
            //OnOffViewCube((VM.AppearanceOpt & OdTvWpfMainWindowModel.AppearanceOptions.ViewCubeEnabled) == OdTvWpfMainWindowModel.AppearanceOptions.ViewCubeEnabled);
            //OnOffFPS((VM.AppearanceOpt & OdTvWpfMainWindowModel.AppearanceOptions.FPSEnabled) == OdTvWpfMainWindowModel.AppearanceOptions.FPSEnabled);
            //OnOffWCS((VM.AppearanceOpt & OdTvWpfMainWindowModel.AppearanceOptions.WCSEnabled) == OdTvWpfMainWindowModel.AppearanceOptions.WCSEnabled);
            //OnOffAnimation((VM.AppearanceOpt & OdTvWpfMainWindowModel.AppearanceOptions.UseAnimation) == OdTvWpfMainWindowModel.AppearanceOptions.UseAnimation);

            //selectDragger.ObjectSelected += VM.TvObjectExplorer.SelectObject;
            //selectDragger.ObjectsSelected += VM.TvPropertiesPalette.ShowSelectionInfo;

            //VM.TvObjectExplorer.ResetEvent += VM.TvPropertiesPalette.FillObjectParameters;

            MM.StopTransaction(mtr);
        }

        public OdTvExtendedView GetActiveTvExtendedView()
        {
            OdTvExtendedView exView = null;
            MemoryTransaction mtr = MM.StartTransaction();
            OdTvGsViewId viewId = _tvDeviceId.openObject().viewAt(TvActiveViewport);
            if (viewId.isNull())
                return null;
            OdTvResult rc = OdTvResult.tvOk;
            OdTvGsView view = viewId.openObject();
            ulong handle = view.getDatabaseHandle(ref rc);

            if (_extendedViewDict.ContainsKey(handle))
                exView = _extendedViewDict[handle];
            else
            {
                exView = new OdTvExtendedView(viewId);
                //exView.setAnimationEnabled((VM.AppearanceOpt & OdTvWpfMainWindowModel.AppearanceOptions.UseAnimation) == OdTvWpfMainWindowModel.AppearanceOptions.UseAnimation);
                exView.setZoomScale(VM.ZoomStep);
                exView.setAnimationDuration(0.9);

                if (view != null)
                {
                    OdGeBoundBlock3d lastExt = new OdGeBoundBlock3d();
                    if (view.getLastViewExtents(lastExt))
                        exView.setViewExtentsForCaching(lastExt);
                }

                _extendedViewDict.Add(handle, exView);
            }

            MM.StopTransaction(mtr);

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

        #region View settings methods

        public void SetRenderMode(OdTvGsView.RenderMode renderMode)
        {
            MemoryTransaction mtr = MM.StartTransaction();
            if (_tvDeviceId != null && !_tvDeviceId.isNull())
            {
                OdTvGsView view = _tvDeviceId.openObject().viewAt(TvActiveViewport).openObject(OpenMode.kForWrite);
                OdTvGsView.RenderMode oldMode = view.mode();
                if (oldMode != renderMode)
                {
                    view.setMode(renderMode);

                    // set mode for WCS
                    //if (WCS != null && (VM.AppearanceOpt & MainWindowViewModel.AppearanceOptions.WCSEnabled) == MainWindowViewModel.AppearanceOptions.WCSEnabled
                    //    && WCS.IsNeedUpdateWCS(oldMode, renderMode))
                    //    WCS.UpdateWCS();

                    _tvDeviceId.openObject().update();
                    VM.SetRenderModeButton(renderMode);
                }
            }
            MM.StopTransaction(mtr);
        }

        public void SetProjectionType(OdTvGsView.Projection projection)
        {
            MemoryTransaction mtr = MM.StartTransaction();
            if (_tvDeviceId != null && !_tvDeviceId.isNull())
            {
                OdTvGsView view = _tvDeviceId.openObject().viewAt(TvActiveViewport).openObject(OpenMode.kForWrite);
                view.setView(view.position(), view.target(), view.upVector(), view.fieldWidth(), view.fieldHeight(), projection);
                _tvDeviceId.openObject().update();
            }
            MM.StopTransaction(mtr);
        }

        public void SetBackgroundColor(Color color)
        {
            MemoryTransaction mtr = MM.StartTransaction();
            uint iColor = ((uint)(color.R | color.G << 8 | ((color.B) << 16)));
            if (_tvDeviceId != null && !_tvDeviceId.isNull())
            {
                OdTvGsDevice dev = _tvDeviceId.openObject(OpenMode.kForWrite);
                dev.setBackgroundColor(iColor);
                dev.TryAutoRegeneration(_cadRegenFactory).update();
            }
            MM.StopTransaction(mtr);
        }

        public void Regen(OdTvGsDevice.RegenMode rm)
        {
            if (_tvDeviceId == null)
                return;
            MemoryTransaction mtr = MM.StartTransaction();
            OdTvGsDevice dev = _tvDeviceId.openObject();
            dev.regen(rm);
            dev.invalidate();
            dev.TryAutoRegeneration(_cadRegenFactory).update();
            MM.StopTransaction(mtr);
        }

        public void Regen()
        {
            if (_tvDeviceId == null)
                return;
            MemoryTransaction mtr = MM.StartTransaction();
            OdTvGsDevice dev = _tvDeviceId.openObject();
            if (TvActiveViewport > 0)
                dev.viewAt(TvActiveViewport).openObject().regen();
            dev.invalidate();
            dev.TryAutoRegeneration(_cadRegenFactory).update();
            MM.StopTransaction(mtr);
        }

        public void Set3DView(OdTvExtendedView.e3DViewType type)
        {
            MemoryTransaction mtr = MM.StartTransaction();

            OdTvExtendedView exView = GetActiveTvExtendedView();
            if (exView == null)
                return;

            // set view type
            exView.setViewType(type);

            //update cached extents if need
            OdGeBoundBlock3d lastExt = new OdGeBoundBlock3d();
            if (!exView.getCachedExtents(lastExt))
            {
                OdTvGsView view = exView.getViewId().openObject();
                if (view.getLastViewExtents(lastExt))
                    exView.setViewExtentsForCaching(lastExt);
            }

            //check existance of the animation
            SetAnimation(exView.getAnimation());

            DisableMarkups();

            Invalidate();

            MM.StopTransaction(mtr);
            if (_serviceFactory.AppSettings.ShowCube)
            {
                OnOffViewCube(true);
            }
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

        #endregion

        #region Draggers methods

        private void ActionAfterDragger(DraggerResult res)
        {
            if ((res & DraggerResult.NeedUpdateCursor) != 0)
                this.Cursor = _dragger.CurrentCursor;

            if ((res & DraggerResult.NeedUpdateView) != 0)
            {
                MemoryTransaction mtr = MM.StartTransaction();
                OdTvGsDevice pDevice = _tvDeviceId.openObject();
                pDevice.TryAutoRegeneration(_cadRegenFactory).update();
                MM.StopTransaction(mtr);
            }

            if ((res & DraggerResult.NeedUFinishDragger) != 0)
                FinishDragger();
        }

        private void StartDragger(ODA.Draggers.OdTvDragger dragger, bool useCurrentAsPrevious = false)
        {
            DraggerResult res = DraggerResult.NothingToDo;

            if (_dragger == null)
                res = dragger.Start(null, TvActiveViewport, Cursor, WCS);
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
                    ActionAfterDragger(res_prev);
                }
                res = dragger.Start(pPrevDragger, TvActiveViewport, Cursor, WCS);
            }
            // need update active dragger before calling action
            _dragger = dragger;
            ActionAfterDragger(res);
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
                    res = _dragger.Start(null, TvActiveViewport, Cursor, WCS);
                    ActionAfterDragger(res);
                }
            }
        }

        private void DisableMarkups()
        {
            if (_tvMarkupModelId != null && _dbId != null)
            {
                MemoryTransaction mtr = MM.StartTransaction();

                OdTvModel pMarkupModel = _tvMarkupModelId.openObject(OpenMode.kForWrite);
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
                        OdTvEntity pEn = entityId.openObject(OpenMode.kForWrite);
                        if (pEn.getName() == OdTvMarkupDragger.NameOfMarkupTempEntity) // if temp entity
                        {
                            pMarkupModel.removeEntity(entityId);
                        }
                        else if (pEn.getVisibility().getType() != OdTvVisibilityDef.VisibilityType.kInvisible)
                        {
                            OdTvGeometryDataIterator pItF = pEn.getGeometryDataIterator();
                            // folds
                            while (!pItF.done())
                            {
                                // objects
                                OdTvEntity pFold = pItF.getGeometryData().openAsSubEntity(OpenMode.kForWrite);
                                OdTvGeometryDataIterator pItO = pFold.getGeometryDataIterator();

                                while (!pItO.done())
                                {
                                    OdTvGeometryDataId geomId = pItO.getGeometryData();
                                    OdTvUserData usrData = geomId.openAsSubEntity(OpenMode.kForWrite).getUserData(AppTvId);
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

        #region Mouse and key down events

        private void TextInputEvent(object sender, KeyPressEventArgs e)
        {
            if ((int)e.KeyChar == (int)Keys.Back || (int)e.KeyChar == (int)Keys.Escape || (int)e.KeyChar == (int)Keys.Enter)
                return;
            if (_dragger != null)
            {
                DraggerResult? res = _dragger.ProcessText(e.KeyChar.ToString());
                if (res == DraggerResult.NeedUpdateView)
                    ActionAfterDragger(DraggerResult.NeedUpdateView);
            }
        }

        private void MouseWheelEvent(object sender, MouseEventArgs e)
        {
            if (_dbId == null || _tvDeviceId == null)
                return;

            VM.UncheckDraggersBtns();

            MemoryTransaction mtr = MM.StartTransaction();

            FinishDragger();

            using var dev = _tvDeviceId.openObject(OpenMode.kForWrite);
            OdTvGsViewId viewId = dev.viewAt(TvActiveViewport);
            OdTvGsView pView = viewId.openObject(OpenMode.kForWrite);
            if (pView == null)
                return;

            OdGePoint2d point = new OdGePoint2d(e.X, e.Y);

            OdGePoint3d pos = new OdGePoint3d(pView.position());
            pos.transformBy(pView.worldToDeviceMatrix());

            int vx, vy;
            vx = (int)pos.x;
            vy = (int)pos.y;

            vx = (int)point.x - vx;
            vy = (int)point.y - vy;

            double scale = 0.9; // wheel down
            if (e.Delta > 0)
                scale = 1.0 / scale; // wheel up

            ScreenDolly(vx, vy);
            pView.zoom(scale);
            ScreenDolly(-vx, -vy);

            dev.TryAutoRegeneration(_cadRegenFactory).update();

            if (_dragger != null)
                _dragger.NotifyAboutViewChange(DraggerViewChangeType.ViewChangeZoom);

            GetActiveTvExtendedView().setViewType(OdTvExtendedView.e3DViewType.kCustom);

            MM.StopTransaction(mtr);
        }

        private void MouseDownEvent(object sender, MouseEventArgs e)
        {
            if (_dragger is OdTvOrbitDragger)
            {
                OnOffViewCube(false);
            }

            _mouseDown = e.Button == MouseButtons.Left ? MouseDownState.LeftMouseBtn :
                e.Button == MouseButtons.Middle ? MouseDownState.MiddleMouseBtn : MouseDownState.None;

            OdTvExtendedView extView = GetActiveTvExtendedView();
            if (_mouseDown == MouseDownState.LeftMouseBtn && extView != null && extView.getEnabledViewCube())
            {
                if (extView.viewCubeProcessClick(e.X, e.Y))
                {
                    if (extView.getAnimationEnabled())
                    {
                        SetAnimation(extView.getAnimation());
                        Invalidate();
                    }
                    return;
                }
            }

            if (_dragger == null) return;

            if (_mouseDown == MouseDownState.MiddleMouseBtn)
            {
                VM.UncheckDraggersBtns();
                if (!_isShiftPressed)
                    Pan();
                else
                    Orbit();
            }

            // activation first
            DraggerResult res = _dragger.Activate();
            ActionAfterDragger(res);
            res = _dragger.NextPoint(e.X, e.Y);
            ActionAfterDragger(res);
        }

        private void MouseUpEvent(object sender, MouseEventArgs e)
        {
            if (_dragger != null)
            {
                DraggerResult res = _dragger.NextPointUp(e.X, e.Y);
                ActionAfterDragger(res);
                if (_mouseDown == MouseDownState.MiddleMouseBtn)
                    FinishDragger();
            }

            _mouseDown = MouseDownState.None;
        }

        private void MouseMoveEvent(object sender, MouseEventArgs e)
        {
            OdTvExtendedView extView = GetActiveTvExtendedView();
            if (extView != null && extView.getEnabledViewCube())
            {
                extView.viewCubeProcessHover(e.X, e.Y);
            }

            if (_dragger != null)
            {
                if ((e.Button == MouseButtons.Left) || (e.Button == MouseButtons.Middle) || _dragger.NeedFreeDrag)
                {
                    DraggerResult res = _dragger.Drag(e.X, e.Y);
                    ActionAfterDragger(res);
                }
            }
        }

        #endregion

        #region Navigation commands

        public void Pan()
        {
            if (_dbId == null || _tvDeviceId == null)
                return;
            //             if (_dragger != null && (_dragger as OdTvSelectDragger) == null)
            //                 FinishDragger();

            ODA.Draggers.OdTvDragger newDragger = new OdTvPanDragger(_tvDeviceId, _tvDraggersModelId);
            StartDragger(newDragger, true);
        }

        public void Orbit()
        {
            if (_dbId == null || _tvDeviceId == null)
                return;

            //             if (_dragger != null && (_dragger as OdTvSelectDragger) == null)
            //                 FinishDragger();

            ODA.Draggers.OdTvDragger newDragger = new OdTvOrbitDragger(_tvDeviceId, _tvDraggersModelId);
            StartDragger(newDragger, true);

            DisableMarkups();
        }

        public void ZoomToArea(bool enable)
        {

        }

        public void Zoom(ZoomType type)
        {
            if (_dbId == null || _tvDeviceId == null)
                return;

            VM.UncheckDraggersBtns();

            MemoryTransaction mtr = MM.StartTransaction();

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
                        OdGeBoundBlock3d lastExt = new OdGeBoundBlock3d();
                        if (!exView.getCachedExtents(lastExt))
                        {
                            OdTvGsView view = exView.getViewId().openObject();
                            if (view.getLastViewExtents(lastExt))
                                exView.setViewExtentsForCaching(lastExt);
                        }
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException(type.ToString() + " (" + type.GetType().ToString() + ")", type, null);
            }

            SetAnimation(exView.getAnimation());
            exView.setViewType(OdTvExtendedView.e3DViewType.kCustom);

            if (_dragger != null)
                _dragger.NotifyAboutViewChange(DraggerViewChangeType.ViewChangeZoom);

            Invalidate();

            MM.StopTransaction(mtr);
        }

        private void ScreenDolly(int x, int y)
        {
            MemoryTransaction mtr = MM.StartTransaction();
            OdTvGsViewId viewId = _tvDeviceId.openObject().viewAt(TvActiveViewport);
            OdTvGsView pView = viewId.openObject();
            if (pView == null)
                return;

            OdGeVector3d vec = new OdGeVector3d(x, y, 0);
            vec.transformBy((pView.screenMatrix() * pView.projectionMatrix()).inverse());
            pView.dolly(vec);
            MM.StopTransaction(mtr);
        }

        #endregion

        #region File commands

        public void LoadFile(string filepath)
        {
            MemoryTransaction mtr = MM.StartTransaction();
            this.Cursor = Cursors.WaitCursor;
            OdTvFactoryId factId = TV_Globals.odTvGetFactory();
            factId.clearDatabases();

            bool isIfc = false;
            try
            {
                using var importparam = GetImportParams(filepath, ref isIfc);
                importparam.setFilePath(filepath);

                if (System.IO.Path.GetExtension(filepath) == ".vsfx")
                    TvDatabaseId = factId.readVSFX(filepath);
                else
                    TvDatabaseId = factId.importFile(importparam);
            }
            catch
            {
                MessageBox.Show("Import failed!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Cursor = Cursors.Default;
                return;
            }

            if (_dbId != null)
            {
                OdTvDatabase odTvDatabase = _dbId.openObject(OpenMode.kForWrite);
                _tvActiveModelId = odTvDatabase == null ? null : odTvDatabase.getModelsIterator().getModel();
                if (_tvActiveModelId == null)
                {
                    MessageBox.Show("Import failed!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Cursor = Cursors.Default;
                    MM.StopTransaction(mtr);
                    if (VM.FileIsExist)
                    {
                        VM.ClearRenderArea();
                    }

                    return;
                }
                if (_serviceFactory.AppSettings.EnableImportUnitChange)
                {
                    SetCadDbUnitAsCadFileUnit(odTvDatabase);
                }
                else
                {
                    GetCadDbUnitAsCadFileUnit(odTvDatabase);
                }
                //SetCadDbUnitAsCadFileUnit(odTvDatabase);

                OdTvDevicesIterator devIt = odTvDatabase.getDevicesIterator();
                if (devIt != null && !devIt.done())
                {
                    _tvDeviceId = devIt.getDevice();
                    OdTvGsDevice odTvGsDevice = _tvDeviceId.openObject(OpenMode.kForWrite);
                    IntPtr wndHndl = new IntPtr(this.Handle.ToInt32());
                    OdTvDCRect rect = new OdTvDCRect(0, Width, Height, 0);
                    odTvGsDevice.setupGs(wndHndl, rect, OdTvGsDevice.Name.kOpenGLES2);
                    odTvGsDevice.setForbidImageHighlight(_serviceFactory.AppSettings.SetForbidImageHighlight);
                    odTvGsDevice.setOption(OdTvGsDevice.Options.kForcePartialUpdate, _serviceFactory.AppSettings.UseForcePartialUpdate);
                    odTvGsDevice.setOption(OdTvGsDevice.Options.kBlocksCache, _serviceFactory.AppSettings.UseBlocksCache);

                    if (!isIfc)
                    {
                        odTvGsDevice.setOption(OdTvGsDevice.Options.kUseSceneGraph, _serviceFactory.AppSettings.UseSceneGraph);
                    }
                    else
                    {
                        odTvGsDevice.setOption(OdTvGsDevice.Options.kUseSceneGraph, _serviceFactory.AppSettings.IfcUseSceneGraph);
                    }

                    for (int i = 0; i < odTvGsDevice.numViews(); i++)
                    {
                        MemoryTransaction viewTr = MM.StartTransaction();
                        if (odTvGsDevice.viewAt(i).openObject().getActive())
                            TvActiveViewport = i;
                        MM.StopTransaction(viewTr);
                    }
                    if (TvActiveViewport < 0)
                    {
                        TvActiveViewport = 0;
                        odTvGsDevice.viewAt(0).openObject(OpenMode.kForWrite).setActive(true);
                    }

                    odTvGsDevice.onSize(rect);
                    odTvGsDevice.TryAutoRegeneration(_cadRegenFactory).update();
                    OdTvGsViewId activeViewId = odTvGsDevice.viewAt(0);
                    ConfigureViewSettings(activeViewId);
                    TvActiveViewport = 0;
                    SetFrozenLayersVisible(odTvDatabase);
                } // Means we have aldready a file and trying to create a new device.
                else if (devIt != null && devIt.done())
                {
                    _tvDeviceId = CreateNewDevice();
                    Stopwatch timer = Stopwatch.StartNew();
                    timer.Start();
                    _tvDeviceId.openObject().update();
                    timer.Stop();
                    DatabaseInfo.FirstUpdateTime = timer.ElapsedMilliseconds;
                }

                FilePath = filepath;
                VM.FileIsExist = true;
                //Init();
                //VM.AppMainWindow.ModelBrowser.Initialize(_dbId, this);
            }

            this.Cursor = Cursors.Default;
            MM.StopTransaction(mtr);
        }
        public uint GetCadDbUnitAsCadFileUnit(OdTvDatabase odTvDatabase)
        {
            //Set model units in the database
            using var modelsIterator = odTvDatabase.getModelsIterator();
            for (; !modelsIterator.done(); modelsIterator.step())
            {
                using var odTvModelId = modelsIterator.getModel();
                using var odTvModel = odTvModelId.openObject(OpenMode.kForRead);
                if (!odTvModelId.isNull())
                {
                    var modelUnits = odTvModel.getUnits();
                    double userDefCoef = 0;
                    if (modelUnits == Units.kUserDefined)
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
            SurveyUnits surveyUnit = _serviceFactory.AppSettings.CadFileUnit;
            CadUnits = UnitsValueConverter.ConvertUnits(surveyUnit, out var isMetric);

            //Set model units in the database
            using var modelsIterator = odTvDatabase.getModelsIterator();
            for (; !modelsIterator.done(); modelsIterator.step())
            {
                using var odTvModelId = modelsIterator.getModel();
                using var odTvModel = odTvModelId.openObject(OpenMode.kForWrite);
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
                using var layer = layersIt.getLayer().openObject(OpenMode.kForWrite);
                if (layer.getTotallyInvisible())
                {
                    layer.setTotallyInvisible(false);
                }
            }
        }

        public void ConfigureViewSettings(OdTvGsViewId bitmapDeviceViewId)
        {
            using var viewOpen = bitmapDeviceViewId.openObject(OpenMode.kForWrite);
            viewOpen.setDefaultLightingIntensity(1.25);

            using var color = new OdTvColorDef(CADModelConstants.LightColor.R, CADModelConstants.LightColor.G,
                                         CADModelConstants.LightColor.B);
            viewOpen.setDefaultLightingColor(color);
            viewOpen.enableDefaultLighting(true, OdTvGsView.DefaultLightingType.kTwoLights);
            using var db = viewOpen.getDatabase().openObject(OpenMode.kForWrite);
            using var background = db.createBackground("Gradient", OdTvGsViewBackgroundId.BackgroundTypes.kGradient);

            using var bg = background.openAsGradientBackground(OpenMode.kForWrite);
            if (bg != null)
            {
                bg.setColorTop(new OdTvColorDef(CADModelConstants.GradientColorTop.R, CADModelConstants.GradientColorTop.G, CADModelConstants.GradientColorTop.B));
                bg.setColorMiddle(new OdTvColorDef(CADModelConstants.GradientColorMiddle.R, CADModelConstants.GradientColorMiddle.G, CADModelConstants.GradientColorMiddle.B));
                bg.setColorBottom(new OdTvColorDef(CADModelConstants.GradientColorBottom.R, CADModelConstants.GradientColorBottom.G, CADModelConstants.GradientColorBottom.B));
                viewOpen.setBackground(background);
            }
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
                    tvIfcImportParams.setFeedbackForChooseCallback(PCallback);
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

        public void SaveFile(string filePath)
        {
            this.Cursor = Cursors.WaitCursor;
            if (_dbId == null)
            {
                MessageBox.Show("There is no database for the save", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                MemoryTransaction mtr = MM.StartTransaction();
                OdTvDatabase db = _dbId.openObject(OpenMode.kForWrite);
                OdTvResult rc = db.writeFile(filePath);
                MM.StopTransaction(mtr);
            }
            catch
            {
                MessageBox.Show("Saving of file\n'" + filePath + "'\n was failed", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            this.Cursor = Cursors.Default;
            FilePath = filePath;
        }

        public void CreateNewFile()
        {
            MemoryTransaction mtr = MM.StartTransaction();
            this.Cursor = Cursors.WaitCursor;
            OdTvFactoryId factId = TV_Globals.odTvGetFactory();
            factId.clearDatabases();
            DatabaseInfo = new TvDatabaseInfo();
            Stopwatch timer = Stopwatch.StartNew();
            timer.Start();
            _dbId = factId.createDatabase();
            try
            {
                OdTvResult rc = OdTvResult.tvCannotOpenFile;
                OdTvDatabase pDatabase = _dbId.openObject(OpenMode.kForWrite, ref rc);
                // Create model
                _tvActiveModelId = pDatabase.createModel("Tv_Model", OdTvModel.Type.kMain);
            }
            catch
            {
                MessageBox.Show("Cannot create new file!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            timer.Stop();
            DatabaseInfo.TvCreationTime = timer.ElapsedMilliseconds;

            _tvDeviceId = CreateNewDevice();
            Init();

            timer.Restart();
            _tvDeviceId.openObject().update();
            timer.Stop();
            DatabaseInfo.FirstUpdateTime = timer.ElapsedMilliseconds;

            VM.FileIsExist = true;
            FilePath = "";
            this.Cursor = Cursors.Default;
            //VM.MainWindow.ModelBrowser.Initialize(_dbId, this);
            MM.StopTransaction(mtr);
        }

        private OdTvGsDeviceId CreateNewDevice()
        {
            MemoryTransaction mtr = MM.StartTransaction();
            OdTvGsDeviceId newDevId = null;

            try
            {
                IntPtr wndHndl = new IntPtr(Handle.ToInt32());
                OdTvDCRect rect = new OdTvDCRect(0, Size.Width, Size.Height, 0);
                newDevId = _dbId.openObject().createDevice("TV_Device", wndHndl, rect, OdTvGsDevice.Name.kOpenGLES2);
                // Open device
                OdTvGsDevice pDevice = newDevId.openObject(OpenMode.kForWrite);
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
                OdTvGsView viewPtr = newViewId.openObject(OpenMode.kForWrite);

                // Setup view to make it contr directional with the WCS normal
                viewPtr.setView(new OdGePoint3d(0, 0, 1), new OdGePoint3d(0, 0, 0), new OdGeVector3d(0, 1, 0), 1, 1);

                // Add main model to the view
                viewPtr.addModel(_tvActiveModelId);

                // Set current view as active
                viewPtr.setActive(true);

                // Set the render mode
                viewPtr.setMode(OdTvGsView.RenderMode.k2DOptimized);

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

            OdTvDatabase db = _dbId.openObject(OpenMode.kForWrite);
            if (db == null)
            {
                MessageBox.Show("There is no database for the save!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MM.StopTransaction(mtr);
                return;
            }
            this.Cursor = Cursors.WaitCursor;

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
                    this.Cursor = Cursors.Default;
                    return;
                }
            }
            catch
            {
                MessageBox.Show("Export of file " + fileName + " was failed!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MM.StopTransaction(mtr);
                this.Cursor = Cursors.Default;
                return;
            }

            this.Cursor = Cursors.Default;
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
                    newDragger = new OdTvPolylineDragger(_tvDeviceId, _tvDraggersModelId, _tvActiveModelId);
                    break;
                case "Ray":
                    newDragger = new OdTvRayDragger(_tvDeviceId, _tvDraggersModelId, _tvActiveModelId);
                    break;
                case "XLine":
                    newDragger = new OdTvXLineDragger(_tvDeviceId, _tvDraggersModelId, _tvActiveModelId);
                    break;
                case "Circle":
                    newDragger = new OdTvCircleDragger(_tvDeviceId, _tvDraggersModelId, _tvActiveModelId);
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
                _tvMarkupModelId = _dbId.openObject(OpenMode.kForWrite).createModel(OdTvMarkupDragger.NameOfMarkupModel, OdTvModel.Type.kDirect, true);
                MM.StopTransaction(mtr);
            }
        }

        public void DrawRectMarkup()
        {
            CreateMarkupModel();
            ODA.Draggers.OdTvDragger newDragger = new OdTvRectMarkupDragger(_tvDeviceId, _tvMarkupModelId);
            StartDragger(newDragger);
        }

        public void DrawCircMarkup()
        {
            CreateMarkupModel();
            ODA.Draggers.OdTvDragger newDragger = new ODA.Draggers.Markups.OdTvCircleMarkupDragger(_tvDeviceId, _tvMarkupModelId);
            StartDragger(newDragger);
        }

        public void DrawHandleMarkup()
        {
            CreateMarkupModel();
            ODA.Draggers.OdTvDragger newDragger = new ODA.Draggers.Markups.OdTvHandleMarkupDragger(_tvDeviceId, _tvMarkupModelId);
            StartDragger(newDragger);
        }

        public void DrawCloudMarkup()
        {
            CreateMarkupModel();
            ODA.Draggers.OdTvDragger newDragger = new ODA.Draggers.Markups.OdTvCloudMarkupDragger(_tvDeviceId, _tvMarkupModelId);
            StartDragger(newDragger);
        }

        public void DrawTextMarkup()
        {
            CreateMarkupModel();
            ODA.Draggers.OdTvDragger newDragger = new ODA.Draggers.Markups.OdTvTextMarkupDragger(_tvDeviceId, _tvMarkupModelId);
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
                    OdTvVisibilityDef.VisibilityType.kInvisible)
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

            SaveMarkupDialog dlg = new SaveMarkupDialog(_tvMarkupModelId, activeEntityId, _tvDeviceId.openObject().viewAt(TvActiveViewport));
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

            LoadMarkupDialog dlg = new LoadMarkupDialog(_tvMarkupModelId, _tvDeviceId.openObject().viewAt(TvActiveViewport), this);
            if (dlg.ShowDialog() == true)
                Invalidate();

            MM.StopTransaction(mtr);
        }

        #endregion

        #region Appearance commands

        public void OnOffWCS(bool bEnable)
        {
            if (_tvDeviceId == null)
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
            MemoryTransaction mtr = MM.StartTransaction();
            _tvDeviceId.openObject().update();
            Invalidate();
            MM.StopTransaction(mtr);
        }

        private void CreateWCS()
        {
            if (WCS != null)
                WCS.removeWCS();

            MemoryTransaction mtr = MM.StartTransaction();
            WCS = new TvWpfViewWCS(TvDatabaseId, TvGsDeviceId.openObject().viewAt(TvActiveViewport));
            OdTvGsView pWcsView = WCS.GetWcsViewId().openObject(OpenMode.kForWrite);
            OdTvGsView activeView = WCS.GetParentViewId().openObject();
            OdGePoint3d activeViewPos = activeView.position();
            //Identity Matrix
            OdGeMatrix3d wcsMatrix = new OdGeMatrix3d();
            //World matrix translated to active Camera's target position.
            wcsMatrix.setTranslation(-activeView.target().asVector());
            //Create a camera view for WCS
            pWcsView.setView(activeViewPos.transformBy(wcsMatrix), OdGePoint3d.kOrigin, activeView.upVector(), 1, 1);
            //pWcsView.setMode(activeView.mode());
            pWcsView.setMode(OdTvGsView.RenderMode.kGouraudShaded);
            pWcsView.zoom(3.2);
            OdGePoint2d lowerLeft = new OdGePoint2d();
            OdGePoint2d upperRight = new OdGePoint2d();
            activeView.getViewport(lowerLeft, upperRight);
            upperRight.x = lowerLeft.x + 1.87;
            upperRight.y = lowerLeft.y + 0.3;
            pWcsView.setViewport(lowerLeft, upperRight);
            WCS.UpdateWCS();

            MM.StopTransaction(mtr);
        }

        public void OnOffViewCube(bool bEnable)
        {
            if (_tvDeviceId == null)
                return;
            MemoryTransaction mtr = MM.StartTransaction();
            OdTvExtendedView extView = GetActiveTvExtendedView();
            if (extView != null && extView.getEnabledViewCube() != bEnable)
            {
                extView.setEnabledViewCube(bEnable);
                Invalidate();
            }
            MM.StopTransaction(mtr);
        }

        public void OnOffFPS(bool bEnable)
        {
            if (_tvDeviceId == null)
                return;
            MemoryTransaction mtr = MM.StartTransaction();
            OdTvGsDevice dev = _tvDeviceId.openObject(OpenMode.kForWrite);
            if (dev.getShowFPS() != bEnable)
            {
                dev.setShowFPS(bEnable);
                dev.TryAutoRegeneration(_cadRegenFactory).update();
                Invalidate();
            }
            MM.StopTransaction(mtr);
        }

        public void OnOffAnimation(bool bEnable)
        {
            if (_tvDeviceId == null)
                return;
            MemoryTransaction mtr = MM.StartTransaction();
            OdTvExtendedView exView = GetActiveTvExtendedView();
            if (exView != null)
                exView.setAnimationEnabled(bEnable);
            MM.StopTransaction(mtr);
        }

        public void SetZoomStep(double dValue)
        {
            if (_tvDeviceId == null || dValue < 1)
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
            if (SelectionSet == null || _tvDeviceId == null || _tvDeviceId.isNull())
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

            Invalidate();

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
            opt.setLevel(OdTvSelectionOptions.Level.kEntity);
            opt.setMode(OdTvSelectionOptions.Mode.kPoint);
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

            Invalidate();
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
            if (_tvDeviceId == null || _tvDeviceId.isNull())
                return;
            MemoryTransaction mtr = MM.StartTransaction();

            OdTvGsView pView = GetActiveTvExtendedView().getViewId().openObject(OpenMode.kForWrite);
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
            OdTvGsView.CuttingPlaneFillStyle oldFillingPatternStyle = OdTvGsView.CuttingPlaneFillStyle.kCheckerboard;
            bool bOldFillingPatternEnabled = pView.getCuttingPlaneFillPatternEnabled(ref oldFillingPatternStyle, out iOldPatternColor);

            bool bNewFillingPatternEnabled = SectioningOptions.FillingPatternEnabled;
            OdTvGsView.CuttingPlaneFillStyle newFillingPatternStyle = SectioningOptions.FillingPaternStyle;
            uint iNewFillingPatternColor = SectioningOptions.FillingPatternColor;

            if (bNewFillingPatternEnabled != bOldFillingPatternEnabled
              || newFillingPatternStyle != oldFillingPatternStyle
              || iNewFillingPatternColor != iOldPatternColor)
                pView.setCuttingPlaneFillPatternEnabled(bNewFillingPatternEnabled, newFillingPatternStyle, iNewFillingPatternColor);

            OdTvGsDevice pDevice = _tvDeviceId.openObject();
            if (pDevice != null)
            {
                pDevice.invalidate();
                pDevice.TryAutoRegeneration(_cadRegenFactory).update();
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
                        ODA.Draggers.OdTvDragger pNewDragger = new ODA.Draggers.OdTvCuttingPlaneDragger(_tvDeviceId, _tvDraggersModelId, this);
                        if (pNewDragger != null)
                            StartDragger(pNewDragger, true);
                        Invalidate();
                    }
                }
            }
            else
            {
                if (_dragger is ODA.Draggers.OdTvCuttingPlaneDragger)
                {
                    ODA.Draggers.OdTvCuttingPlaneDragger cuttingPlaneDragger = (ODA.Draggers.OdTvCuttingPlaneDragger)_dragger;
                    if (cuttingPlaneDragger != null)
                    {
                        cuttingPlaneDragger.IsCanFinish = true;
                        FinishDragger();
                        Invalidate();
                    }
                }
            }
        }

        public bool ShowCuttingPlanes()
        {
            MemoryTransaction mtr = MM.StartTransaction();
            SectioningOptions.IsShown = !SectioningOptions.IsShown;

            bool bRet = false;

            OdTvGsView pActiveView = GetActiveTvExtendedView().getViewId().openObject(OpenMode.kForWrite);
            OdTvGsDevice pDevice = _tvDeviceId.openObject(OpenMode.kForWrite);
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
                ODA.Draggers.OdTvDragger pNewDragger = new ODA.Draggers.OdTvCuttingPlaneDragger(_tvDeviceId, _tvDraggersModelId, this);
                if (pNewDragger != null)
                    StartDragger(pNewDragger, true);

                pDevice.invalidate();

                bRet = true;
            }
            else
            {
                if (_dragger is ODA.Draggers.OdTvCuttingPlaneDragger)
                {
                    // finish the dragger
                    ODA.Draggers.OdTvCuttingPlaneDragger cuttingPlaneDragger = (ODA.Draggers.OdTvCuttingPlaneDragger)_dragger;
                    cuttingPlaneDragger.IsCanFinish = true;
                    FinishDragger();
                }

                // remove geometry for the sectioning planes
                if (!_cuttingPlanesModelId.isNull())
                {
                    OdTvModel pMoveModel = _cuttingPlanesModelId.openObject(OpenMode.kForWrite);
                    if (pMoveModel != null)
                        pMoveModel.clearEntities();
                }

                // remove view as sibling from an active
                pActiveView.removeSibling(_cuttingPlanesViewId);
                // remove view from device
                pDevice.removeView(_cuttingPlanesViewId);
                bRet = true;
            }

            Invalidate();
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

            OdTvModel pCuttingPlaneModel = _cuttingPlanesModelId.openObject(OpenMode.kForWrite);
            // create cutting plane entity
            OdTvEntityId cuttingPlanesEntityId = pCuttingPlaneModel.appendEntity("$_CUTTINGPLANE_ENTITY" + index);
            //set a few parameters to the cutting plane
            OdTvEntity pCuttingPlanesEntity = cuttingPlanesEntityId.openObject(OpenMode.kForWrite);
            pCuttingPlanesEntity.setColor(new OdTvColorDef(175, 175, 175));
            pCuttingPlanesEntity.setLineWeight(new OdTvLineWeightDef(ODA.Draggers.OdTvCuttingPlaneDragger.OD_TV_CUTTINGPLANE_EDGE_DEFAULT_LINEWEIGHT));
            pCuttingPlanesEntity.setTransparency(new OdTvTransparencyDef(0.8));
            pCuttingPlanesEntity.addViewDependency(_cuttingPlanesViewId);

            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(index));
            Marshal.WriteInt32(ptr, 0, index);
            OdTvByteUserData data = new OdTvByteUserData(ptr, sizeof(int), OdTvByteUserData.Ownership.kCopyOwn, false);

            pCuttingPlanesEntity.appendUserData(data, AppTvId);

            // Calculate points for cutting plane shell
            OdGePoint3d origin = new OdGePoint3d();
            OdGeVector3d uAxis = new OdGeVector3d();
            OdGeVector3d vAxis = new OdGeVector3d();
            cuttingPlane.get(origin, uAxis, vAxis);

            // Get max distance between extents
            double cuttingPlaneSize = getMainModelExtentsDistance() / 2d;

            OdTvPointArray points = new OdTvPointArray();

            OdGeMatrix3d transformMatrix = new OdGeMatrix3d();
            // 0
            OdGeVector3d moveVector0 = -vAxis - uAxis;
            moveVector0 = moveVector0 * cuttingPlaneSize * ODA.Draggers.OdTvCuttingPlaneDragger.OD_TV_CUTTINGPLANE_SIZE_COEFF;
            transformMatrix.setToIdentity();
            transformMatrix.setToTranslation(moveVector0);
            OdGePoint3d point0 = new OdGePoint3d(origin);
            point0 = point0.transformBy(transformMatrix);
            points.Add(point0);

            // 1
            OdGeVector3d moveVector1 = vAxis - uAxis;
            moveVector1 = moveVector1 * cuttingPlaneSize * ODA.Draggers.OdTvCuttingPlaneDragger.OD_TV_CUTTINGPLANE_SIZE_COEFF;
            transformMatrix.setToIdentity();
            transformMatrix.setToTranslation(moveVector1);
            OdGePoint3d point1 = new OdGePoint3d(origin);
            point1 = point1.transformBy(transformMatrix);
            points.Add(point1);

            // 2
            OdGeVector3d moveVector2 = vAxis + uAxis;
            moveVector2 = moveVector2 * cuttingPlaneSize * ODA.Draggers.OdTvCuttingPlaneDragger.OD_TV_CUTTINGPLANE_SIZE_COEFF;
            transformMatrix.setToIdentity();
            transformMatrix.setToTranslation(moveVector2);
            OdGePoint3d point2 = new OdGePoint3d(origin);
            point2 = point2.transformBy(transformMatrix);
            points.Add(point2);

            // 3
            OdGeVector3d moveVector3 = uAxis - vAxis;
            moveVector3 = moveVector3 * cuttingPlaneSize * ODA.Draggers.OdTvCuttingPlaneDragger.OD_TV_CUTTINGPLANE_SIZE_COEFF;
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
            if (_dragger is ODA.Draggers.OdTvCuttingPlaneDragger)
            {
                ODA.Draggers.OdTvCuttingPlaneDragger cuttingPlaneDragger = (ODA.Draggers.OdTvCuttingPlaneDragger)_dragger;
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

            OdTvGsView pActiveView = GetActiveTvExtendedView().getViewId().openObject(OpenMode.kForWrite);
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
                OdTvGsDevice pDevice = _tvDeviceId.openObject(OpenMode.kForWrite);
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

                pDevice.invalidate();
                Invalidate();
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
            OdTvGsView pActiveView = GetActiveTvExtendedView().getViewId().openObject(OpenMode.kForWrite);
            if (pActiveView == null)
            {
                MM.StopTransaction(mtr);
                return;
            }

            try
            {
                OdTvGsDevice pDevice = _tvDeviceId.openObject(OpenMode.kForWrite);
                if (SectioningOptions.IsShown)
                {
                    //notify dragger
                    if (_dragger is ODA.Draggers.OdTvCuttingPlaneDragger)
                    {
                        ODA.Draggers.OdTvCuttingPlaneDragger cuttingPlaneDragger = (ODA.Draggers.OdTvCuttingPlaneDragger)_dragger;
                        if (cuttingPlaneDragger != null)
                            cuttingPlaneDragger.OnRemoveCuttingPlanes();
                    }

                    // remove geometry for the sectioning planes
                    if (!_cuttingPlanesModelId.isNull())
                    {
                        OdTvModel pMoveModel = _cuttingPlanesModelId.openObject(OpenMode.kForWrite);
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
                pDevice.invalidate();
                Invalidate();

            }
            catch (System.Exception)
            {
            }

            MM.StopTransaction(mtr);
        }

        public void UpdateCadView(bool invalidate = false)
        {
            throw new NotImplementedException();
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
}

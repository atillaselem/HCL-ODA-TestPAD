using HCL_ODA_TestPAD.Dialogs;
using HCL_ODA_TestPAD.HCL;
using HCL_ODA_TestPAD.HCL.CadUnits;
using HCL_ODA_TestPAD.ODA.Draggers;
using HCL_ODA_TestPAD.ODA.Draggers.Construct;
using HCL_ODA_TestPAD.ODA.Draggers.Markups;
using HCL_ODA_TestPAD.ODA.Draggers.Navigation;
using HCL_ODA_TestPAD.ODA.ModelBrowser;
using HCL_ODA_TestPAD.ODA.WCS;
using HCL_ODA_TestPAD.Settings;
using HCL_ODA_TestPAD.ViewModels;
using HCL_ODA_TestPAD.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ODA.Kernel.TD_RootIntegrated;
using ODA.Visualize.TV_Visualize;
using ODA.Visualize.TV_VisualizeTools;

namespace HCL_ODA_TestPAD.UserControls
{
    public partial class WinFormsCadImageViewControl : UserControl, IOdaSectioning, IOpenGles2Control
    {
        private readonly IServiceFactory _serviceFactory;
        public WinFormsCadImageViewControl Adapter => this;
        #region App Specific Variables
        public MainWindowViewModel Vm { get; set; }
        public string FilePath { get; private set; }
        //public double Width { get; set; }
        //public double Height { get; set; }
        public bool AddDefaultViewOnLoad { get; set; }
        #endregion

        #region Image Bufferring Variables

        static readonly int WidthResized;
        static readonly int HeightResized;
        #endregion

        #region Unit Variables
        private UnitsValue CadUnits { get; set; }
        #endregion

        #region ODA Visualize Variables
        private readonly MemoryManager _mm = MemoryManager.GetMemoryManager();

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
        private int _cuttingPlaneNum = 0;
        public OdTvSectioningOptions SectioningOptions { get; set; }
        public static int _odTvCuttingPlaneMaxNum = 5;

        public static OdTvRegAppId AppTvId { get; set; }
        private Dictionary<ulong, OdTvExtendedView> _extendedViewDict = new Dictionary<ulong, OdTvExtendedView>();

        private OdTvAnimation _animation = null;

        // list with selected items(bold nodes in model browser)
        public List<TvTreeItem> SelectedNodes = new List<TvTreeItem>();

        public OdTvSelectionSet SelectionSet = null;
        public TvWpfViewWcs Wcs { get; set; }
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
            Vm = vm;
            Size = new Size((int)Vm.AppMainWindow.Width, (int)Vm.AppMainWindow.Height);
            SectioningOptions = new ODA.Draggers.OdTvSectioningOptions();

            var cadGenerator = new CadRegenerator(serviceFactory);
            _cadRegenFactory = () => cadGenerator;
        }

        static WinFormsCadImageViewControl()
        {
            WidthResized = 0;
            HeightResized = 0;
        }

        public void ClearDevices()
        {
            MemoryTransaction mtr = _mm.StartTransaction();

            foreach (var extView in _extendedViewDict)
            {
                extView.Value.Dispose();
            }

            _extendedViewDict.Clear();

            if (_dbId != null && !_dbId.isNull() && _dbId.isValid())
                _dbId.openObject(OdTv_OpenMode.kForWrite).clearDevices();

            _mm.StopTransaction(mtr);
        }

        #region Paint and initialization

        private void OdTvWpfView_Paint(object sender, PaintEventArgs e)
        {
            if (!this.Disposing && _tvDeviceId != null && !_tvDeviceId.isNull())
            {
                MemoryTransaction mtr = _mm.StartTransaction();

                OdTvGsDevice pDevice = _tvDeviceId.openObject();
                pDevice.TryAutoRegeneration(_cadRegenFactory).update();

                if (_animation != null && _animation.isRunning())
                {
                    _animation.step();
                    Invalidate();
                    if (_dragger != null && !_animation.isRunning())
                        _dragger.NotifyAboutViewChange(DraggerViewChangeType.ViewChangeFull);
                }

                _mm.StopTransaction(mtr);
            }
        }

        private void ResizePanel(object sender, EventArgs e)
        {
            if (_tvDeviceId != null && !this.Disposing)
            {
                MemoryTransaction mtr = _mm.StartTransaction();
                OdTvGsDevice dev = _tvDeviceId.openObject(OdTv_OpenMode.kForWrite);
                if (this.Width > 0 && this.Height > 0)
                {
                    dev.onSize(new OdTvDCRect(0, Width, Height, 0));
                    dev.TryAutoRegeneration(_cadRegenFactory).update();
                }
                _mm.StopTransaction(mtr);
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e) { }

        private void Init()
        {
            if (_dbId == null || _tvDeviceId == null)
                return;
            MemoryTransaction mtr = _mm.StartTransaction();
            OdTvDatabase pDb = _dbId.openObject(OdTv_OpenMode.kForWrite);
            _tvDraggersModelId = pDb.createModel("Draggers", OdTvModel_Type.kDirect, false);
            OdTvSelectDragger selectDragger = new OdTvSelectDragger(this, _tvActiveModelId, _tvDeviceId, _tvDraggersModelId);
            _dragger = selectDragger;
            DraggerResult res = _dragger.Start(null, TvActiveViewport, Cursor, Wcs);
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
            _cuttingPlanesModelId = pDb.createModel("$ODA_TVVIEWER_SECTIONING_MODEL_" + _cuttingPlaneNum, OdTvModel_Type.kMain, false);
            _cuttingPlanesViewId = TvGsDeviceId.openObject(OdTv_OpenMode.kForWrite).createView("$ODA_TVVIEWER_SECTIONING_VIEW_" + _cuttingPlaneNum++);
            OdTvGsView pVsectioningView = _cuttingPlanesViewId.openObject(OdTv_OpenMode.kForWrite);
            pVsectioningView.addModel(_cuttingPlanesModelId);
            pVsectioningView.setMode(OdTvGsView_RenderMode.kGouraudShaded);

            // set projection button
            OdTvGsView view = _tvDeviceId.openObject().viewAt(TvActiveViewport).openObject();
            if (view.isPerspective())
                Vm.AppMainWindow.PerspectiveBtn.IsChecked = true;
            else
                Vm.AppMainWindow.IsometricBtn.IsChecked = true;
            // set render mode
            Vm.SetRenderModeButton(view.mode());

            //VM.AppMainWindow.PropertiesPalette.InitializePalette(_tvDevice, this);

            //// enable or disable wcs, fps and grid
            //OnOffViewCube((VM.AppearanceOpt & OdTvWpfMainWindowModel.AppearanceOptions.ViewCubeEnabled) == OdTvWpfMainWindowModel.AppearanceOptions.ViewCubeEnabled);
            //OnOffFPS((VM.AppearanceOpt & OdTvWpfMainWindowModel.AppearanceOptions.FPSEnabled) == OdTvWpfMainWindowModel.AppearanceOptions.FPSEnabled);
            //OnOffWCS((VM.AppearanceOpt & OdTvWpfMainWindowModel.AppearanceOptions.WCSEnabled) == OdTvWpfMainWindowModel.AppearanceOptions.WCSEnabled);
            //OnOffAnimation((VM.AppearanceOpt & OdTvWpfMainWindowModel.AppearanceOptions.UseAnimation) == OdTvWpfMainWindowModel.AppearanceOptions.UseAnimation);

            //selectDragger.ObjectSelected += VM.TvObjectExplorer.SelectObject;
            //selectDragger.ObjectsSelected += VM.TvPropertiesPalette.ShowSelectionInfo;

            //VM.TvObjectExplorer.ResetEvent += VM.TvPropertiesPalette.FillObjectParameters;

            _mm.StopTransaction(mtr);
        }

        public OdTvExtendedView GetActiveTvExtendedView()
        {
            OdTvExtendedView exView = null;
            MemoryTransaction mtr = _mm.StartTransaction();
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
                exView.setZoomScale(Vm.ZoomStep);
                exView.setAnimationDuration(0.9);

                if (view != null)
                {
                    OdGeBoundBlock3d lastExt = new OdGeBoundBlock3d();
                    if (view.getLastViewExtents(lastExt))
                        exView.setViewExtentsForCaching(lastExt);
                }

                _extendedViewDict.Add(handle, exView);
            }

            _mm.StopTransaction(mtr);

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

        public void SetRenderMode(OdTvGsView_RenderMode renderMode)
        {
            MemoryTransaction mtr = _mm.StartTransaction();
            if (_tvDeviceId != null && !_tvDeviceId.isNull())
            {
                OdTvGsView view = _tvDeviceId.openObject().viewAt(TvActiveViewport).openObject(OdTv_OpenMode.kForWrite);
                OdTvGsView_RenderMode oldMode = view.mode();
                if (oldMode != renderMode)
                {
                    view.setMode(renderMode);

                    // set mode for WCS
                    //if (WCS != null && (VM.AppearanceOpt & MainWindowViewModel.AppearanceOptions.WCSEnabled) == MainWindowViewModel.AppearanceOptions.WCSEnabled
                    //    && WCS.IsNeedUpdateWCS(oldMode, renderMode))
                    //    WCS.UpdateWCS();

                    _tvDeviceId.openObject().update();
                    Vm.SetRenderModeButton(renderMode);
                }
            }
            _mm.StopTransaction(mtr);
        }

        public void SetProjectionType(OdTvGsView_Projection projection)
        {
            MemoryTransaction mtr = _mm.StartTransaction();
            if (_tvDeviceId != null && !_tvDeviceId.isNull())
            {
                OdTvGsView view = _tvDeviceId.openObject().viewAt(TvActiveViewport).openObject(OdTv_OpenMode.kForWrite);
                view.setView(view.position(), view.target(), view.upVector(), view.fieldWidth(), view.fieldHeight(), projection);
                _tvDeviceId.openObject().update();
            }
            _mm.StopTransaction(mtr);
        }

        public void SetBackgroundColor(Color color)
        {
            MemoryTransaction mtr = _mm.StartTransaction();
            uint iColor = ((uint)(color.R | color.G << 8 | ((color.B) << 16)));
            if (_tvDeviceId != null && !_tvDeviceId.isNull())
            {
                OdTvGsDevice dev = _tvDeviceId.openObject(OdTv_OpenMode.kForWrite);
                dev.setBackgroundColor(iColor);
                dev.TryAutoRegeneration(_cadRegenFactory).update();
            }
            _mm.StopTransaction(mtr);
        }

        public void Regen(OdTvGsDevice_RegenMode rm)
        {
            if (_tvDeviceId == null)
                return;
            MemoryTransaction mtr = _mm.StartTransaction();
            OdTvGsDevice dev = _tvDeviceId.openObject();
            dev.regen(rm);
            dev.invalidate();
            dev.TryAutoRegeneration(_cadRegenFactory).update();
            _mm.StopTransaction(mtr);
        }

        public void Regen()
        {
            if (_tvDeviceId == null)
                return;
            MemoryTransaction mtr = _mm.StartTransaction();
            OdTvGsDevice dev = _tvDeviceId.openObject();
            if (TvActiveViewport > 0)
                dev.viewAt(TvActiveViewport).openObject().regen();
            dev.invalidate();
            dev.TryAutoRegeneration(_cadRegenFactory).update();
            _mm.StopTransaction(mtr);
        }

        public void Set3DView(OdTvExtendedView_e3DViewType type)
        {
            MemoryTransaction mtr = _mm.StartTransaction();

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

            _mm.StopTransaction(mtr);
            if (_serviceFactory.AppSettings.ShowCube)
            {
                OnOffViewCube(true);
            }
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

        #endregion

        #region Draggers methods

        private void ActionAfterDragger(DraggerResult res)
        {
            if ((res & DraggerResult.NeedUpdateCursor) != 0)
                this.Cursor = _dragger.CurrentCursor;

            if ((res & DraggerResult.NeedUpdateView) != 0)
            {
                MemoryTransaction mtr = _mm.StartTransaction();
                OdTvGsDevice pDevice = _tvDeviceId.openObject();
                pDevice.TryAutoRegeneration(_cadRegenFactory).update();
                _mm.StopTransaction(mtr);
            }

            if ((res & DraggerResult.NeedUFinishDragger) != 0)
                FinishDragger();
        }

        private void StartDragger(ODA.Draggers.OdTvDragger dragger, bool useCurrentAsPrevious = false)
        {
            DraggerResult res = DraggerResult.NothingToDo;

            if (_dragger == null)
                res = dragger.Start(null, TvActiveViewport, Cursor, Wcs);
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
                    ActionAfterDragger(resPrev);
                }
                res = dragger.Start(pPrevDragger, TvActiveViewport, Cursor, Wcs);
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
                    res = _dragger.Start(null, TvActiveViewport, Cursor, Wcs);
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

            Vm.UncheckDraggersBtns();

            MemoryTransaction mtr = _mm.StartTransaction();

            FinishDragger();

            using var dev = _tvDeviceId.openObject(OdTv_OpenMode.kForWrite);
            OdTvGsViewId viewId = dev.viewAt(TvActiveViewport);
            OdTvGsView pView = viewId.openObject(OdTv_OpenMode.kForWrite);
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

            GetActiveTvExtendedView().setViewType(OdTvExtendedView_e3DViewType.kCustom);

            _mm.StopTransaction(mtr);
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
                Vm.UncheckDraggersBtns();
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

            Vm.UncheckDraggersBtns();

            MemoryTransaction mtr = _mm.StartTransaction();

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
            exView.setViewType(OdTvExtendedView_e3DViewType.kCustom);

            if (_dragger != null)
                _dragger.NotifyAboutViewChange(DraggerViewChangeType.ViewChangeZoom);

            Invalidate();

            _mm.StopTransaction(mtr);
        }

        public void ShowTool(HclToolType type)
        {
            throw new NotImplementedException("HCL Tools are not implemented for GPU Device.");
        }

        private void ScreenDolly(int x, int y)
        {
            MemoryTransaction mtr = _mm.StartTransaction();
            OdTvGsViewId viewId = _tvDeviceId.openObject().viewAt(TvActiveViewport);
            OdTvGsView pView = viewId.openObject();
            if (pView == null)
                return;

            OdGeVector3d vec = new OdGeVector3d(x, y, 0);
            vec.transformBy((pView.screenMatrix() * pView.projectionMatrix()).inverse());
            pView.dolly(vec);
            _mm.StopTransaction(mtr);
        }

        #endregion

        #region File commands

        public void LoadFile(string filepath)
        {
            MemoryTransaction mtr = _mm.StartTransaction();
            this.Cursor = Cursors.WaitCursor;
            OdTvFactoryId factId = TV_Visualize_Globals.odTvGetFactory();
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
                OdTvDatabase odTvDatabase = _dbId.openObject(OdTv_OpenMode.kForWrite);
                _tvActiveModelId = odTvDatabase == null ? null : odTvDatabase.getModelsIterator().getModel();
                if (_tvActiveModelId == null)
                {
                    MessageBox.Show("Import failed!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Cursor = Cursors.Default;
                    _mm.StopTransaction(mtr);
                    if (Vm.FileIsExist)
                    {
                        Vm.ClearRenderArea();
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
                    OdTvGsDevice odTvGsDevice = _tvDeviceId.openObject(OdTv_OpenMode.kForWrite);
                    IntPtr wndHndl = new IntPtr(this.Handle.ToInt32());
                    OdTvDCRect rect = new OdTvDCRect(0, Width, Height, 0);
                    odTvGsDevice.setupGs(wndHndl, rect, OdTvGsDevice_Name.kOpenGLES2);
                    odTvGsDevice.setForbidImageHighlight(_serviceFactory.AppSettings.SetForbidImageHighlight);
                    odTvGsDevice.setOption(OdTvGsDevice_Options.kForcePartialUpdate, _serviceFactory.AppSettings.UseForcePartialUpdate);
                    odTvGsDevice.setOption(OdTvGsDevice_Options.kBlocksCache, _serviceFactory.AppSettings.UseBlocksCache);

                    if (!isIfc)
                    {
                        odTvGsDevice.setOption(OdTvGsDevice_Options.kUseSceneGraph, _serviceFactory.AppSettings.UseSceneGraph);
                    }
                    else
                    {
                        odTvGsDevice.setOption(OdTvGsDevice_Options.kUseSceneGraph, _serviceFactory.AppSettings.IfcUseSceneGraph);
                    }

                    for (int i = 0; i < odTvGsDevice.numViews(); i++)
                    {
                        MemoryTransaction viewTr = _mm.StartTransaction();
                        if (odTvGsDevice.viewAt(i).openObject().getActive())
                            TvActiveViewport = i;
                        _mm.StopTransaction(viewTr);
                    }
                    if (TvActiveViewport < 0)
                    {
                        TvActiveViewport = 0;
                        odTvGsDevice.viewAt(0).openObject(OdTv_OpenMode.kForWrite).setActive(true);
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
                Vm.FileIsExist = true;
                //Init();
                //VM.AppMainWindow.ModelBrowser.Initialize(_dbId, this);
            }

            OnOffFPS(true);
            this.Cursor = Cursors.Default;
            _mm.StopTransaction(mtr);
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
            SurveyUnits surveyUnit = _serviceFactory.AppSettings.CadFileUnit;
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
                    dwgPmtrs.setDCRect(new OdTvDCRect(0, (int)WidthResized, (int)HeightResized, 0));
                    dwgPmtrs.setObjectNaming(_serviceFactory.AppSettings.DwgSetObjectNaming);
                    dwgPmtrs.setStoreSourceObjects(_serviceFactory.AppSettings.DwgSetStoreSourceObjects);
                    dwgPmtrs.setFeedbackForChooseCallback(null);
                    dwgPmtrs.setImportFrozenLayers(_serviceFactory.AppSettings.DwgSetImportFrozenLayers);
                    dwgPmtrs.setClearEmptyObjects(_serviceFactory.AppSettings.DwgSetClearEmptyObjects);
                    dwgPmtrs.setNeedCDATree(_serviceFactory.AppSettings.DwgSetNeedCdaTree);
                    dwgPmtrs.setNeedCollectPropertiesInCDA(_serviceFactory.AppSettings.DwgSetNeedCollectPropertiesInCda);
                }
                else if (ext == ".ifc")
                {
                    var tvIfcImportParams = new OdTvIfcImportParams();
                    tvIfcImportParams.setNeedCDATree(_serviceFactory.AppSettings.IfcSetNeedCdaTree);
                    tvIfcImportParams.setNeedCollectPropertiesInCDA(_serviceFactory.AppSettings.IfcSetNeedCollectPropertiesInCda);
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
                MemoryTransaction mtr = _mm.StartTransaction();
                OdTvDatabase db = _dbId.openObject(OdTv_OpenMode.kForWrite);
                OdTvResult rc = db.writeFile(filePath);
                _mm.StopTransaction(mtr);
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
            MemoryTransaction mtr = _mm.StartTransaction();
            this.Cursor = Cursors.WaitCursor;
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

            _tvDeviceId = CreateNewDevice();
            Init();

            timer.Restart();
            _tvDeviceId.openObject().update();
            timer.Stop();
            DatabaseInfo.FirstUpdateTime = timer.ElapsedMilliseconds;

            Vm.FileIsExist = true;
            FilePath = "";
            this.Cursor = Cursors.Default;
            //VM.MainWindow.ModelBrowser.Initialize(_dbId, this);
            _mm.StopTransaction(mtr);
        }

        private OdTvGsDeviceId CreateNewDevice()
        {
            MemoryTransaction mtr = _mm.StartTransaction();
            OdTvGsDeviceId newDevId = null;

            try
            {
                IntPtr wndHndl = new IntPtr(Handle.ToInt32());
                OdTvDCRect rect = new OdTvDCRect(0, Size.Width, Size.Height, 0);
                newDevId = _dbId.openObject().createDevice("TV_Device", wndHndl, rect, OdTvGsDevice_Name.kOpenGLES2);
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
                    _mm.StopTransaction(mtr);
                    this.Cursor = Cursors.Default;
                    return;
                }
            }
            catch
            {
                MessageBox.Show("Export of file " + fileName + " was failed!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _mm.StopTransaction(mtr);
                this.Cursor = Cursors.Default;
                return;
            }

            this.Cursor = Cursors.Default;
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
                MemoryTransaction mtr = _mm.StartTransaction();
                _tvMarkupModelId = _dbId.openObject(OdTv_OpenMode.kForWrite).createModel(OdTvMarkupDragger.NameOfMarkupModel, OdTvModel_Type.kDirect, true);
                _mm.StopTransaction(mtr);
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

            SaveMarkupDialog dlg = new SaveMarkupDialog(_tvMarkupModelId, activeEntityId, _tvDeviceId.openObject().viewAt(TvActiveViewport));
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

            LoadMarkupDialog dlg = new LoadMarkupDialog(_tvMarkupModelId, _tvDeviceId.openObject().viewAt(TvActiveViewport), this);
            if (dlg.ShowDialog() == true)
                Invalidate();

            _mm.StopTransaction(mtr);
        }

        #endregion

        #region Appearance commands

        public void OnOffWCS(bool bEnable)
        {
            if (_tvDeviceId == null)
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
            MemoryTransaction mtr = _mm.StartTransaction();
            _tvDeviceId.openObject().update();
            Invalidate();
            _mm.StopTransaction(mtr);
        }

        private void CreateWcs()
        {
            if (Wcs != null)
                Wcs.RemoveWcs();

            MemoryTransaction mtr = _mm.StartTransaction();
            Wcs = new TvWpfViewWcs(TvDatabaseId, TvGsDeviceId.openObject().viewAt(TvActiveViewport));
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
            pWcsView.zoom(3.2);
            OdGePoint2d lowerLeft = new OdGePoint2d();
            OdGePoint2d upperRight = new OdGePoint2d();
            activeView.getViewport(lowerLeft, upperRight);
            upperRight.x = lowerLeft.x + 1.87;
            upperRight.y = lowerLeft.y + 0.3;
            pWcsView.setViewport(lowerLeft, upperRight);
            Wcs.UpdateWcs();

            _mm.StopTransaction(mtr);
        }

        public void OnOffViewCube(bool bEnable)
        {
            if (_tvDeviceId == null)
                return;
            MemoryTransaction mtr = _mm.StartTransaction();
            OdTvExtendedView extView = GetActiveTvExtendedView();
            if (extView != null && extView.getEnabledViewCube() != bEnable)
            {
                extView.setEnabledViewCube(bEnable);
                Invalidate();
            }
            _mm.StopTransaction(mtr);
        }

        public void OnOffFPS(bool bEnable)
        {
            if (_tvDeviceId == null)
                return;
            MemoryTransaction mtr = _mm.StartTransaction();
            OdTvGsDevice dev = _tvDeviceId.openObject(OdTv_OpenMode.kForWrite);
            if (dev.getShowFPS() != bEnable)
            {
                dev.setShowFPS(bEnable);
                dev.TryAutoRegeneration(_cadRegenFactory).update();
                Invalidate();
            }
            _mm.StopTransaction(mtr);
        }

        public void OnOffAnimation(bool bEnable)
        {
            if (_tvDeviceId == null)
                return;
            MemoryTransaction mtr = _mm.StartTransaction();
            OdTvExtendedView exView = GetActiveTvExtendedView();
            if (exView != null)
                exView.setAnimationEnabled(bEnable);
            _mm.StopTransaction(mtr);
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

            Invalidate();

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

            OdTvGsDevice pDevice = _tvDeviceId.openObject();
            if (pDevice != null)
            {
                pDevice.invalidate();
                pDevice.TryAutoRegeneration(_cadRegenFactory).update();
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
            MemoryTransaction mtr = _mm.StartTransaction();
            SectioningOptions.IsShown = !SectioningOptions.IsShown;

            bool bRet = false;

            OdTvGsView pActiveView = GetActiveTvExtendedView().getViewId().openObject(OdTv_OpenMode.kForWrite);
            OdTvGsDevice pDevice = _tvDeviceId.openObject(OdTv_OpenMode.kForWrite);
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

            Invalidate();
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
            pCuttingPlanesEntity.setLineWeight(new OdTvLineWeightDef(ODA.Draggers.OdTvCuttingPlaneDragger.OdTvCuttingplaneEdgeDefaultLineweight));
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
            moveVector0 = moveVector0 * cuttingPlaneSize * ODA.Draggers.OdTvCuttingPlaneDragger.OdTvCuttingplaneSizeCoeff;
            transformMatrix.setToIdentity();
            transformMatrix.setToTranslation(moveVector0);
            OdGePoint3d point0 = new OdGePoint3d(origin);
            point0 = point0.transformBy(transformMatrix);
            points.Add(point0);

            // 1
            OdGeVector3d moveVector1 = vAxis - uAxis;
            moveVector1 = moveVector1 * cuttingPlaneSize * ODA.Draggers.OdTvCuttingPlaneDragger.OdTvCuttingplaneSizeCoeff;
            transformMatrix.setToIdentity();
            transformMatrix.setToTranslation(moveVector1);
            OdGePoint3d point1 = new OdGePoint3d(origin);
            point1 = point1.transformBy(transformMatrix);
            points.Add(point1);

            // 2
            OdGeVector3d moveVector2 = vAxis + uAxis;
            moveVector2 = moveVector2 * cuttingPlaneSize * ODA.Draggers.OdTvCuttingPlaneDragger.OdTvCuttingplaneSizeCoeff;
            transformMatrix.setToIdentity();
            transformMatrix.setToTranslation(moveVector2);
            OdGePoint3d point2 = new OdGePoint3d(origin);
            point2 = point2.transformBy(transformMatrix);
            points.Add(point2);

            // 3
            OdGeVector3d moveVector3 = uAxis - vAxis;
            moveVector3 = moveVector3 * cuttingPlaneSize * ODA.Draggers.OdTvCuttingPlaneDragger.OdTvCuttingplaneSizeCoeff;
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
                OdTvGsDevice pDevice = _tvDeviceId.openObject(OdTv_OpenMode.kForWrite);
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
                OdTvGsDevice pDevice = _tvDeviceId.openObject(OdTv_OpenMode.kForWrite);
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
                pDevice.invalidate();
                Invalidate();

            }
            catch (System.Exception)
            {
            }

            _mm.StopTransaction(mtr);
        }

        public void UpdateCadView(bool invalidate = false)
        {
            throw new NotImplementedException();
        }
        public void ShowFps()
        {
            OnOffFPS(_serviceFactory.AppSettings.ShowFps);
        }
        public void ShowWcs()
        {
            OnOffWCS(_serviceFactory.AppSettings.ShowWcs);
        }
        public void ShowCube()
        {
            OnOffViewCube(_serviceFactory.AppSettings.ShowCube);
        }

        public void ShowCustomModels()
        {
            ShowFps();
            ShowWcs();
        }

        public void SaveAsVsfx(string fileName)
        {
            throw new NotImplementedException();
        }

        public void ApplyFillColor()
        {
            throw new NotImplementedException();
        }
        public void ShowLayers()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}

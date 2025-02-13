/////////////////////////////////////////////////////////////////////////////// 
// Copyright (C) 2002-2022, Open Design Alliance (the "Alliance"). 
// All rights reserved. 
// 
// This software and its documentation and related materials are owned by 
// the Alliance. The software may only be incorporated into application 
// programs owned by members of the Alliance, subject to a signed 
// Membership Agreement and Supplemental Software License Agreement with the
// Alliance. The structure and organization of this software are the valuable  
// trade secrets of the Alliance and its suppliers. The software is also 
// protected by copyright law and international treaty provisions. Application  
// programs incorporating this software must include the following statement 
// with their copyright notices:
//   
//   This application incorporates Open Design Alliance software pursuant to a license 
//   agreement with Open Design Alliance.
//   Open Design Alliance Copyright (C) 2002-2022 by Open Design Alliance. 
//   All rights reserved.
//
// By use of this software, its documentation or related materials, you 
// acknowledge and accept the above terms.
///////////////////////////////////////////////////////////////////////////////

using HCL_ODA_TestPAD.ViewModels.Base;
using System;
using System.Windows.Forms;
using ODA.Kernel.TD_RootIntegrated;
using ODA.Visualize.TV_Visualize;

namespace HCL_ODA_TestPAD.ODA.Draggers;

public class OdTvSelectDragger : OdTvDragger
{
    public delegate void ObjectSelectedHandler(OdTvSelectionSet sSet, OdTvModelId modelId);
    public event ObjectSelectedHandler ObjectSelected;
    public event ObjectSelectedHandler ObjectsSelected;

    enum SelectState
    {
        KPoint = 0,
        KWindow = 1,
        KCrossing = 2
    };

    public const string TvSelectorView = "$ODA_WPF_TVVIEWER_SELECTORVIEW";
    public const string TvSelectorLinetype = "$ODA_WPF_TVVIEWER_SELECTORLINETYPE";

    //current selection set
    private OdTvSelectionSet _sSet;
    // model for selection
    private OdTvModelId _modelId;
    // dragger state
    private SelectState _state = SelectState.KPoint;
    // first cliked point (Device CS)
    private OdTvDCPoint _firstDevicePt = new OdTvDCPoint();
    // temporary view for draggers geometry
    private OdTvGsViewId _tempViewId;
    // specific linetype for the selector
    private OdTvLinetypeId _frameLinetypeId;
    // temporary geometry
    private OdTvEntityId _entityId;
    private OdTvGeometryDataId _frameId;
    private OdTvGeometryDataId _frameIdContourId;

    private OdTvSelectionOptions _opt = new OdTvSelectionOptions();
    private OdTvDCPoint[] _pts = new OdTvDCPoint[2];

    OdTvGsView _view = null;

    public OdTvSelectDragger(/*IOpenGLES2Control*/ IOdaSectioning wpfView, OdTvModelId modelId, OdTvGsDeviceId tvDeviceId, OdTvModelId tvDraggersModelId)
        : base(tvDeviceId, tvDraggersModelId)
    {
        MemoryTransaction mtr = _mm.StartTransaction();

        _pts[0] = new OdTvDCPoint();
        _pts[1] = new OdTvDCPoint();

        _modelId = modelId;
        // In this dragger we will be used a separate special view for drawing temporary objects like selection rectangles
        // Such technique will allow doesn't depend on the render mode at the current active view. Also we will not have any problems 
        // with linetype scale for our temporary objects
        OdTvGsDevice dev = tvDeviceId.openObject(OdTv_OpenMode.kForWrite);
        _tempViewId = dev.createView(TvSelectorView, false);

        // Get specific linetype for the selection rectangle boundary
        OdTvDatabaseId dbId = _modelId.openObject().getDatabase();
        _frameLinetypeId = dbId.openObject().findLinetype(TvSelectorLinetype);
        if (_frameLinetypeId.isNull())
        {
            // Create custom linetype for the selection rectangle boundary
            OdTvLinetypeElement dash = OdTvLinetypeDashElement.createObject(0.25);
            OdTvLinetypeElementPtr ltDash = new OdTvLinetypeElementPtr(dash, OdRxObjMod.kOdRxObjAttach);
            OdTvLinetypeElement space = OdTvLinetypeSpaceElement.createObject(0.25);
            OdTvLinetypeElementPtr ltSpace = new OdTvLinetypeElementPtr(space, OdRxObjMod.kOdRxObjAttach);
            OdTvLinetypeElementArray ltArr = new OdTvLinetypeElementArray() { ltDash, ltSpace };

            _frameLinetypeId = dbId.openObject(OdTv_OpenMode.kForWrite).createLinetype(TvSelectorLinetype, ltArr);

            GC.SuppressFinalize(ltDash);
            GC.SuppressFinalize(ltSpace);
        }
        _mm.StopTransaction(mtr);
    }

    ~OdTvSelectDragger()
    {
        if (_view != null)
            _view.Dispose();
    }

    public override DraggerResult NextPoint(int x, int y)
    {
        DraggerResult rc = DraggerResult.NothingToDo;
        //first of all we need the active view to perform selection
        if (_view == null && TvView != null)
        {
            _view = TvView.openObject(OdTv_OpenMode.kForWrite);
        }

        //perform selection
        if (_state == SelectState.KPoint)
        {
            //remember first click
            _firstDevicePt.x = x;
            _firstDevicePt.y = y;
            _pts[0] = new OdTvDCPoint(_firstDevicePt.x, _firstDevicePt.y);
            _pts[1] = new OdTvDCPoint(_firstDevicePt.x, _firstDevicePt.y);
            //update base color
            UpdateBaseColor();
            _opt.setMode(OdTvSelectionOptions_Mode.kPoint);
        }
        else
        {
            //remember second click
            _pts[1] = new OdTvDCPoint(x, y);

            if (_state == SelectState.KWindow)
                _opt.setMode(OdTvSelectionOptions_Mode.kWindow);
            else
                _opt.setMode(OdTvSelectionOptions_Mode.kCrossing);

            // setup temporary view and model
            DisableTemporaryObjects();

            //mark that we need to update the view
            rc = DraggerResult.NeedUpdateView;
        }

        // prepare data for the selection call
        OdTvSelectionSet pSSet = _view.select(_pts, _opt, _modelId);

        //check selection results
        if (pSSet != null && pSSet.numItems() == 0 && _state == SelectState.KPoint) // start window or crossing mode
        {
            _state = SelectState.KWindow;

            // setup temporary view and model
            EnableTemporaryObjects();

            //mark that we want to receive drag without pressed mouse button
            NeedFreeDrag = true;
        }
        else if (_state != SelectState.KPoint)
            _state = SelectState.KPoint;

        if (pSSet != null && pSSet.numItems() != 0)
        {
            // merge new selection set with current
            Merge(pSSet);

            //call window update
            rc = DraggerResult.NeedUpdateView;
        }

        return rc;
    }

    public override DraggerResult Drag(int x, int y)
    {
        if (_state == SelectState.KPoint)
            return DraggerResult.NothingToDo;

        //filter coordinates
        //if (x >= _wpfView.Width || x < 1 || y >= _wpfView.Height || y < 1)
        //    return DraggerResult.NothingToDo;
        MemoryTransaction mtr = _mm.StartTransaction();
        // create temporary geometry if need
        OdTvEntity entity = null;
        if (_entityId != null && !_entityId.isNull())
            entity = _entityId.openObject(OdTv_OpenMode.kForWrite);
        UpdateFrame(entity == null, x, y);
        _mm.StopTransaction(mtr);

        return DraggerResult.NeedUpdateView;
    }

    public override DraggerResult ProcessEscape()
    {
        if (_state != SelectState.KPoint)
            return DraggerResult.NothingToDo;

        //unhighlight current selection set
        Highlight(_sSet, false);
        MemoryTransaction mtr = _mm.StartTransaction();
        TvDeviceId.openObject().update();
        _mm.StopTransaction(mtr);

        //remove current selection set
        if (_sSet != null)
            _sSet.Dispose();
        _sSet = null;
        _pts[0] = new OdTvDCPoint();
        _pts[1] = new OdTvDCPoint();

        return DraggerResult.NeedUpdateView;
    }

    public override bool UpdateCursor()
    {
        CurrentCursor = State == DraggerState.Finishing ? LasAppActiveCursor : Cursors.Arrow;
        return true;
    }

    private void EnableTemporaryObjects()
    {
        MemoryTransaction mtr = _mm.StartTransaction();
        //get device
        OdTvGsDevice dev = TvDeviceId.openObject(OdTv_OpenMode.kForWrite);
        if (dev == null)
        {
            _mm.StopTransaction(mtr);
            return;
        }

        //add temporary view
        dev.addView(_tempViewId);

        //get view ptr
        OdTvGsView view = _tempViewId.openObject(OdTv_OpenMode.kForWrite);
        if (view == null)
        {
            _mm.StopTransaction(mtr);
            return;
        }

        //setup view to make it contr directional with the WCS normal
        view.setView(new OdGePoint3d(0, 0, 1), new OdGePoint3d(0, 0, 0), new OdGeVector3d(0, 1, 0), 1, 1);

        //add draggers model to view
        view.addModel(TvDraggerModelId);
        _mm.StopTransaction(mtr);
    }

    private void DisableTemporaryObjects()
    {
        MemoryTransaction mtr = _mm.StartTransaction();
        //remove view from the device
        OdTvGsDevice dev = TvDeviceId.openObject(OdTv_OpenMode.kForWrite);
        dev.removeView(_tempViewId);
        //erase draggers model fromview
        _tempViewId.openObject(OdTv_OpenMode.kForWrite).eraseModel(TvDraggerModelId);
        //remove entities from the temporary model
        TvDraggerModelId.openObject(OdTv_OpenMode.kForWrite).clearEntities();
        dev.update();
        _mm.StopTransaction(mtr);
    }

    private void UpdateFrame(bool bCreate, int x, int y)
    {
        MemoryTransaction mtr = _mm.StartTransaction();

        OdGePoint3d[] pts = new OdGePoint3d[5];

        pts[0] = ToEyeToWorldLocal(_firstDevicePt.x, _firstDevicePt.y);
        pts[2] = ToEyeToWorldLocal(x, y);
        pts[4] = new OdGePoint3d(pts[0]);

        OdTvGsView pLocalView = _tempViewId.openObject();
        if (pLocalView == null)
        {
            _mm.StopTransaction(mtr);
            return;
        }

        OdGeMatrix3d matr = pLocalView.viewingMatrix();
        OdGePoint3d p0 = matr * pts[0];
        OdGePoint3d p2 = matr * pts[2];

        pts[1] = new OdGePoint3d(p0.x, p2.y, p2.z);
        pts[3] = new OdGePoint3d(p2.x, p0.y, p2.z);

        bool bCrossing = p0.x > p2.x;

        _state = SelectState.KWindow;
        if (bCrossing)
            _state = SelectState.KCrossing;

        matr = pLocalView.eyeToWorldMatrix();
        pts[1].transformBy(matr);
        pts[3].transformBy(matr);

        //update or create entity
        if (bCreate)
        {
            OdTvModel model = TvDraggerModelId.openObject(OdTv_OpenMode.kForWrite);
            _entityId = model.appendEntity();
            {
                OdTvEntity entityNew = _entityId.openObject(OdTv_OpenMode.kForWrite);

                //create frame
                _frameId = entityNew.appendPolygon(new[] { pts[0], pts[1], pts[2], pts[3] });
                _frameId.openObject().setTransparency(new OdTvTransparencyDef(0.7));
                _frameId.openAsPolygon().setFilled(true);

                _frameIdContourId = entityNew.appendPolyline(pts);
                _frameIdContourId.openObject().setColor(TvDraggerColor);

                if (bCrossing)
                {
                    entityNew.setColor(new OdTvColorDef(0, 255, 0));
                    entityNew.setLinetypeScale(0.02);
                    _frameIdContourId.openObject().setLinetype(new OdTvLinetypeDef(_frameLinetypeId));
                }
                else
                    entityNew.setColor(new OdTvColorDef(0, 0, 255));
            }
        }
        else
        {
            OdTvGeometryData frame = _frameId.openObject();
            if (frame == null || frame.getType() != OdTv_OdTvGeometryDataType.kPolygon)
            {
                _mm.StopTransaction(mtr);
                return;
            }

            OdTvPolygonData polygon = frame.getAsPolygon();
            polygon.setPoints(new[] { pts[0], pts[1], pts[2], pts[3] });

            _frameIdContourId.openAsPolyline().setPoints(pts);

            if (bCrossing)
            {
                _entityId.openObject(OdTv_OpenMode.kForWrite).setColor(new OdTvColorDef(0, 255, 0));
                _entityId.openObject(OdTv_OpenMode.kForWrite).setLinetypeScale(0.03);
                _frameIdContourId.openObject().setLinetype(new OdTvLinetypeDef(_frameLinetypeId));
            }
            else
            {
                _entityId.openObject(OdTv_OpenMode.kForWrite).setColor(new OdTvColorDef(0, 0, 255));
                _frameIdContourId.openObject().setLinetype(new OdTvLinetypeDef());
            }
        }
        _mm.StopTransaction(mtr);
    }

    private OdGePoint3d ToEyeToWorldLocal(int x, int y)
    {
        MemoryTransaction mtr = _mm.StartTransaction();
        OdGePoint3d wcsPt = new OdGePoint3d(x, y, 0);
        OdTvGsView view = _tempViewId.openObject();

        if (view.isPerspective())
            wcsPt.z = view.projectionMatrix().GetItem(2, 3);
        wcsPt.transformBy((view.screenMatrix() * view.projectionMatrix()).inverse());
        wcsPt.z = 0;
        //transform to world coordinate system
        wcsPt.transformBy(view.eyeToWorldMatrix());
        _mm.StopTransaction(mtr);
        return wcsPt;
    }

    private void Merge(OdTvSelectionSet sSet)
    {
        if (sSet == null)
            return;

        // always clear previous selection set
        if (_sSet != null)
        {
            Highlight(_sSet, false);
            _sSet.Dispose();
            _sSet = null;
        }

        if (ObjectSelected != null)
        {
            if (sSet.getOptions().getMode() == OdTvSelectionOptions_Mode.kPoint)
                ObjectSelected(sSet, _modelId);
            else
                ObjectsSelected(sSet, _modelId);
        }

        if (sSet.getOptions().getMode() == OdTvSelectionOptions_Mode.kWindow ||
            sSet.getOptions().getMode() == OdTvSelectionOptions_Mode.kCrossing)
        {
            if (_sSet == null)
            {
                _sSet = sSet;
                Highlight(_sSet, true);
                return;
            }
        }
    }

    private void Highlight(OdTvSelectionSet sSet, bool bDoIt)
    {
        if (sSet == null)
            return;
        MemoryTransaction mtr = _mm.StartTransaction();
        OdTvSelectionSetIterator pIter = sSet.getIterator();
        for (; pIter != null && !pIter.done(); pIter.step())
            Highlight(pIter, bDoIt);

        if (ObjectSelected != null)
        {
            if (sSet.getOptions().getMode() == OdTvSelectionOptions_Mode.kPoint)
                ObjectSelected(sSet, _modelId);
            else
                ObjectsSelected(sSet, _modelId);
        }

        _mm.StopTransaction(mtr);
    }

    private void Highlight(OdTvSelectionSetIterator pIter, bool bDoIt)
    {
        MemoryTransaction mtr = _mm.StartTransaction();
        OdTvGsView view = TvView.openObject(OdTv_OpenMode.kForWrite);
        if (view == null)
        {
            _mm.StopTransaction(mtr);
            return;
        }
        //get entity
        OdTvEntityId id = pIter.getEntity();
        //get sub item
        OdTvSubItemPath path = new OdTvSubItemPath();
        pIter.getPath(path);
        //perform highlight
        view.highlight(id, path, bDoIt);

        _mm.StopTransaction(mtr);
    }

}

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

using HCL_ODA_TestPAD.ODA.WCS;
using System.Windows.Forms;
using ODA.Kernel.TD_RootIntegrated;
using ODA.Visualize.TV_Visualize;

namespace HCL_ODA_TestPAD.ODA.Draggers.Markups;

public class OdTvCircleMarkupDragger : OdTvMarkupDragger
{
    // first click point
    private OdGePoint3d _firstPoint = null;
    // last drag point
    private OdGePoint3d _lastDragPoint = null;
    // flag for success exit from dragger
    private bool _isSuccess = false;
    // local state of the dragger
    private bool _isPressed = false;
    // need to control the ::start called first time or not
    private bool _justCreatedObject = true;

    // temporary geometry
    private OdTvEntityId _entityId = null;
    private OdTvGeometryDataId _circFoldId = null;
    private OdTvGeometryDataId _circEntityId = null;
    private OdTvGeometryDataId _circId = null;

    public OdTvCircleMarkupDragger(OdTvGsDeviceId deviceId, OdTvModelId markupModelId)
        : base(deviceId, markupModelId)
    {
        NeedFreeDrag = true;

        MemoryTransaction mTr = _mm.StartTransaction();
        // create main entity
        OdTvModel pModel = markupModelId.openObject(OdTv_OpenMode.kForWrite);
        _entityId = FindMarkupEntity(NameOfMarkupTempEntity, true);
        if (_entityId == null)
        {
            _entityId = pModel.appendEntity(NameOfMarkupTempEntity);
            _entityId.openObject(OdTv_OpenMode.kForWrite).setColor(MarkupColor);
        }

        // crate circle subEntity if not exist
        _circFoldId = FindSubEntity(_entityId.openObject().getGeometryDataIterator(), NameOfMarkupCircFold);
        if (_circFoldId == null)
            _circFoldId = _entityId.openObject(OdTv_OpenMode.kForWrite).appendSubEntity(NameOfMarkupCircFold);

        _mm.StopTransaction(mTr);
    }

    public override DraggerResult Start(OdTvDragger prevDragger, int activeView, Cursor cursor, TvWpfViewWcs wcs)
    {
        TvActiveViewport = activeView;
        DraggerResult res = DraggerResult.NothingToDo;

        // create temporary geometry
        if (!_justCreatedObject)
        {
            UpdateFrame(true);
            res = DraggerResult.NeedUpdateView;
        }

        res = res | base.Start(prevDragger, activeView, cursor, wcs);

        // add dragger model to the view
        AddDraggersModelToView();

        if (!_justCreatedObject)
            State = DraggerState.Working;

        _justCreatedObject = false;

        return res;
    }

    public override DraggerResult NextPoint(int x, int y)
    {
        if (!CheckDragger())
            return DraggerResult.NothingToDo;

        if (!_isPressed)
        {
            _isPressed = true;
            // remember first click
            _firstPoint = ToEyeToWorld(x, y);
        }
        else
        {
            _isPressed = false;
            if (_circEntityId != null)
            {
                MemoryTransaction mtr = _mm.StartTransaction();
                _circEntityId.openAsSubEntity(OdTv_OpenMode.kForWrite).setLineWeight(LineWeight);
                _mm.StopTransaction(mtr);
            }
            return DraggerResult.NeedUpdateView;
        }
        return DraggerResult.NothingToDo;
    }

    public override DraggerResult NextPointUp(int x, int y)
    {
        if (!_isPressed)
        {
            _isSuccess = true;
            return DraggerResult.NeedUFinishDragger | Reset();
        }

        return DraggerResult.NothingToDo;
    }

    public override DraggerResult Drag(int x, int y)
    {
        if (!CheckDragger())
            return DraggerResult.NothingToDo;

        if (!_isPressed)
            return DraggerResult.NothingToDo;

        // get current drag point
        _lastDragPoint = ToEyeToWorld(x, y);

        // create temporary geometry if need
        UpdateFrame(_circEntityId == null);

        return DraggerResult.NeedUpdateView;
    }

    public override bool UpdateCursor()
    {
        CurrentCursor = State == DraggerState.Finishing ? LasAppActiveCursor : Cursors.Cross;
        return true;
    }

    public override OdTvDragger Finish(out DraggerResult rc)
    {
        if (!_isSuccess && _circEntityId != null)
        {
            MemoryTransaction mtr = _mm.StartTransaction();
            _circFoldId.openAsSubEntity(OdTv_OpenMode.kForWrite).removeGeometryData(_circEntityId);
            TvDeviceId.openObject().update();
            _mm.StopTransaction(mtr);
        }

        return base.Finish(out rc);
    }

    private void UpdateFrame(bool isNeedCreate)
    {
        if (TvView == null)
            return;
        MemoryTransaction mTr = _mm.StartTransaction();
        // update or create entity
        if (isNeedCreate)
        {
            OdTvGsView view = TvView.openObject();
            _circEntityId = _circFoldId.openAsSubEntity(OdTv_OpenMode.kForWrite).appendSubEntity();
            OdTvEntity circEnt = _circEntityId.openAsSubEntity(OdTv_OpenMode.kForWrite);
            _circId = circEnt.appendCircle(_firstPoint, (_lastDragPoint - _firstPoint).length(), view.position() - view.target());
        }
        else
        {
            OdTvGeometryData pFrame = _circId.openObject();
            if (pFrame == null || pFrame.getType() != OdTv_OdTvGeometryDataType.kCircle)
                return;
            OdTvCircleData circle = pFrame.getAsCircle();
            circle.setRadius((_lastDragPoint - _firstPoint).length());
        }
        _mm.StopTransaction(mTr);
    }
}

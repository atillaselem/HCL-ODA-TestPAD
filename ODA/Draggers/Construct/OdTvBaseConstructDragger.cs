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
using System;
using System.Windows.Forms;
using Teigha.Core;
using Teigha.Visualize;

namespace HCL_ODA_TestPAD.ODA.Draggers.Construct;

public class OdTvBaseConstructDragger : OdTvDragger
{
    public enum ClickState
    {
        WaitingInfiniteClick = -1,
        WaitingFirstClick = 0,
        WaitingSecondClick = 1,
        WaitingThirdClick = 2,
        WaitingFourthClick = 3,
        WaitingFifthClick = 4
    };

    // local state of the dragger
    protected ClickState _clickState = ClickState.WaitingFirstClick;
    // last state of the dragger (after it the dragger should be finished or restarted)
    protected ClickState _lastState = ClickState.WaitingSecondClick;

    // model for adding created objects
    protected OdTvModelId _activeModelId = null;

    // clicked points
    protected OdGePoint3dArray _clickedPts = null;

    // true if and only if dragger should be restarted after end of work
    protected bool _bDraggerShouldBeRestarted = false;

    // need to control the ::start called first time or not
    protected bool _bFirstStart = true;

    // new geometry
    protected OdTvEntityId _entityId = null;
    protected OdTvGeometryDataId _newGeometryId = null;

    //indicate that the result should be transfered to the active model
    protected bool _bNeedTransferToActive = false;

    public OdTvBaseConstructDragger(OdTvGsDeviceId tvDeviceId, OdTvModelId tvDraggersModelId, OdTvModelId activeModel)
        : base(tvDeviceId, tvDraggersModelId)
    {
        _activeModelId = activeModel;

        NeedFreeDrag = true;
        _clickedPts = new OdGePoint3dArray();
        _clickedPts.resize(5);

        HaveDrawableTemporaryGeometry = true;
    }

    public virtual void UpdateGeometry(bool bCreate, bool bFromNextPoint)
    {

    }

    public override DraggerResult Start(OdTvDragger prevDragger, int activeView, Cursor cursor, TvWpfViewWCS wcs)
    {
        DraggerResult rc = DraggerResult.NothingToDo;

        // create temporary geometry in the case when we restarted from the previous state
        if (!_bFirstStart)
        {
            UpdateGeometry(true, false);
            rc = DraggerResult.NeedUpdateView;
        }

        CurrentCursor = cursor;
        LasAppActiveCursor = cursor;

        rc = rc | base.Start(prevDragger, activeView, cursor, wcs);
        AddDraggersModelToView();

        UpdateBaseColor();

        if (!_bFirstStart)
            State = DraggerState.Working;

        _bFirstStart = false;

        return rc;
    }

    public override DraggerResult NextPoint(int x, int y)
    {
        DraggerResult rc = DraggerResult.NothingToDo;
        OdTvGsView pView = TvView.openObject();
        if (State == DraggerState.Waiting || pView == null)
            return rc;

        if (_clickState == ClickState.WaitingFirstClick)
        {
            // remember the click
            _clickedPts[(int)_clickState] = ToEyeToWorld(x, y);
            ToUcsToWorld(_clickedPts[(int)_clickState]);

            //increase click state
            _clickState = (ClickState)((int)_clickState + 1);
        }
        else
        {
            // remember the click
            _clickedPts[(int)_clickState] = ToEyeToWorld(x, y);
            ToUcsToWorld(_clickedPts[(int)_clickState]);

            // update created geometry
            UpdateGeometry(false, true);

            if (_clickState == _lastState)
            {
                // if it was a dragger with view - we should add the result to the auto regeneration map
                if (_entityId != null)
                {
                    OdTvEntity pEn = _entityId.openObject(OpenMode.kForWrite);
                    if (pEn != null)
                    {
                        pEn.setAutoRegen(true);
                        pEn.Dispose();
                    }
                }

                // release current entity (means final adding to the database)
                _entityId = null;

                // restart or finish the dragger
                if (_bDraggerShouldBeRestarted)
                {
                    TransferResultToActiveModel();
                    _clickedPts[0] = _clickedPts[(int)_clickState];
                    _clickState = ClickState.WaitingSecondClick;
                }
                else
                {
                    _bNeedTransferToActive = true;
                    rc = DraggerResult.NeedUFinishDragger;
                }
            }
            else
                //increase click state
                _clickState = (ClickState)((int)_clickState + 1);

            pView.Dispose();

            return rc | DraggerResult.NeedUpdateView;
        }

        pView.Dispose();

        return rc;
    }

    public override DraggerResult Drag(int x, int y)
    {
        if (_clickState == ClickState.WaitingFirstClick)
            return DraggerResult.NothingToDo;

        // remember the drag
        _clickedPts[(int)_clickState] = ToEyeToWorld(x, y);
        ToUcsToWorld(_clickedPts[(int)_clickState]);

        // create temporary geometry if need
        if (_entityId == null)
            UpdateGeometry(true, false);
        else // update created geometry
            UpdateGeometry(false, false);

        return DraggerResult.NeedUpdateView;
    }

    public override OdTvDragger Finish(out DraggerResult rc)
    {
        //transfer last created entity to the main active model
        if (_bNeedTransferToActive)
        {
            TransferResultToActiveModel();
        }

        DraggerResult rcFinish;
        OdTvDragger retFinish = base.Finish(out rcFinish);

        rc = rcFinish | DraggerResult.NeedUpdateView;
        return retFinish;
    }

    public override bool CanFinish()
    {
        return true;
    }

    public override bool UpdateCursor()
    {
        CurrentCursor = State == DraggerState.Finishing ? LasAppActiveCursor : Cursors.Cross;
        return true;
    }

    protected double GetDistance(OdGePoint3d pt1, OdGePoint3d pt2)
    {
        OdTvGsView pView = TvView.openObject();
        if (pView == null)
            return 0.0;

        //1. Calculate the UCS normal in the device space
        OdGeVector3d vUcsNormal = OdGeVector3d.kZAxis; // currently UCS = WCS
        vUcsNormal.transformBy(pView.worldToDeviceMatrix());

        //2. Calculate the UCS normal in the eye space
        vUcsNormal.normalize();
        vUcsNormal = new OdGeVector3d(vUcsNormal.x, -vUcsNormal.y, vUcsNormal.z);

        //3. Calculate the UCS normal in UCS plane
        OdGePoint3d ptScrO = new OdGePoint3d(0d, 0d, 0d);
        OdGePoint3d ptScrV = new OdGePoint3d(vUcsNormal.x, vUcsNormal.y, vUcsNormal.z);

        ptScrO.transformBy(pView.eyeToWorldMatrix());
        ptScrV.transformBy(pView.eyeToWorldMatrix());

        ToUcsToWorld(ptScrO);
        ToUcsToWorld(ptScrV);

        vUcsNormal = (ptScrV - ptScrO).normalize();

        //4. Project the vector between two clicked points into the UCS normal in UCS plane
        double dist = (pt2 - pt1).dotProduct(vUcsNormal);

        //5. Calculate special coefficient
        OdGeVector3d vUcsDirection = OdGeVector3d.kZAxis; // currently UCS = WCS
        OdGeVector3d vDirection = pView.position() - pView.target();

        // is in perpendicular plane to UCS one
        vUcsDirection.normalize();
        vDirection.normalize();
        double dang = vDirection.angleTo(vUcsDirection);
        double dcoef = Math.Tan(dang);
        if (dcoef != 0d)
            dist /= dcoef;

        //6. Sometimes it is need to change the sign
        OdGeVector3d vTst = OdGeVector3d.kXAxis + OdGeVector3d.kYAxis;
        double scalar = vDirection.dotProduct(vTst);
        if (scalar != 0d)
        {
            vTst.x *= 0.9;
            vTst.y *= 1.1;
        }

        if (vDirection.dotProduct(vTst) < 0.0 == vUcsNormal.dotProduct(vTst) < 0.0)
            dist = -dist;

        pView.Dispose();

        return dist;
    }

    public virtual void TransferResultToActiveModel()
    {
        if (_activeModelId == null || _newGeometryId == null)
            return;

        OdTvModel model = _activeModelId.openObject(OpenMode.kForWrite);
        OdTvEntityId entityId = model.appendEntity();
        OdTvEntity pEn = entityId.openObject(OpenMode.kForWrite);
        pEn.setColor(TvDraggerColor);

        switch (_newGeometryId.getType())
        {
            case OdTvGeometryDataType.kPolyline:
                {
                    OdTvPolylineData pLine = _newGeometryId.openAsPolyline();
                    OdTvPointArray arr = new OdTvPointArray();
                    pLine.getPoints(arr);
                    pEn.appendPolyline(arr);
                    pLine.Dispose();
                    break;
                }
            case OdTvGeometryDataType.kCircle:
                {
                    OdTvCircleData pCirc = _newGeometryId.openAsCircle();
                    OdGePoint3d center = new OdGePoint3d();
                    double radius = 0d;
                    OdGeVector3d normal = new OdGeVector3d();
                    pCirc.get(center, out radius, normal);
                    pEn.appendCircle(center, radius, normal);
                    pCirc.Dispose();
                    break;
                }
            case OdTvGeometryDataType.kInfiniteLine:
                {
                    OdTvInfiniteLineData pInf = _newGeometryId.openAsInfiniteLine();
                    pEn.appendInfiniteLine(pInf.getFirst(), pInf.getSecond(), pInf.getType());
                    pInf.Dispose();
                    break;
                }
        }

        pEn.Dispose();
        model.Dispose();
    }
}

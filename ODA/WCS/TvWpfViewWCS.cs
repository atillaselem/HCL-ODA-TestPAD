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
using ODA.Kernel.TD_RootIntegrated;
using ODA.Visualize.TV_Visualize;

namespace HCL_ODA_TestPAD.ODA.WCS;

public class TvWpfViewWcs
{
    // Special model for WCS object
    private OdTvModelId _tvWcsModelId;

    // View, in which this WCS is located
    private OdTvGsViewId _activeViewId;

    // View, associated with this view
    private OdTvGsViewId _wcsViewId;

    private static int _wcsViewNumber = 1;

    private MemoryManager _mm = MemoryManager.GetMemoryManager();

    public TvWpfViewWcs(OdTvDatabaseId tvDbId, OdTvGsViewId tvViewId)
    {
        _activeViewId = tvViewId;

        MemoryTransaction mtr = _mm.StartTransaction();
        OdTvGsDevice odTvGsDevice = _activeViewId.openObject().device().openObject(OdTv_OpenMode.kForWrite);

        // add wcs view
        OdTvResult rc = new OdTvResult();
        rc = OdTvResult.tvOk;
        _wcsViewId = odTvGsDevice.createView("WcsView_" + _wcsViewNumber, false, ref rc);
        odTvGsDevice.addView(_wcsViewId);

        _tvWcsModelId = tvDbId.openObject(OdTv_OpenMode.kForWrite).createModel("$ODA_TVVIEWER_WCS_" + _wcsViewNumber++);
        OdTvGsView wcsView = _wcsViewId.openObject(OdTv_OpenMode.kForWrite);
        wcsView.addModel(_tvWcsModelId);

        _mm.StopTransaction(mtr);
    }
    
    public void UpdateWcs()
    {
        MemoryTransaction mtr = _mm.StartTransaction();

        OdTvGsView view = _activeViewId.openObject();

        // remove old wcs entities
        OdTvModel wcsModel = _tvWcsModelId.openObject(OdTv_OpenMode.kForWrite);
        wcsModel.clearEntities();

        //1.1 Add wcs entity
        OdTvEntityId wcsEntityId = wcsModel.appendEntity("WCS");
        OdTvEntity wcsEntity = wcsEntityId.openObject(OdTv_OpenMode.kForWrite);

        // define the start point for the WCS
        OdGePoint3d start = new OdGePoint3d(0d, 0d, 0d);

        // caculate axis lines length in wireframe and shaded modes
        double lineLength = 0.07;
        if ((int)view.mode() != (int)OdGsView_RenderMode.kWireframe && (int)view.mode() != (int)OdGsView_RenderMode.k2DOptimized)
            lineLength = 0.07;

        // create X axis and label
        OdTvGeometryDataId wcsX = wcsEntity.appendSubEntity("wcs_x");
        OdGePoint3d endx = new OdGePoint3d(start);
        endx.x += lineLength;
        CreateWcsAxis(wcsX, new OdTvColorDef(189, 19, 19), start, endx, "X");

        // create Y axis and label
        OdTvGeometryDataId wcsY = wcsEntity.appendSubEntity("wcs_y");
        OdGePoint3d endy = new OdGePoint3d(start);
        endy.y += lineLength;
        CreateWcsAxis(wcsY, new OdTvColorDef(12, 171, 20), start, endy, "Y");

        // create Z axis and label
        OdTvGeometryDataId wcsZ = wcsEntity.appendSubEntity("wcs_z");
        OdGePoint3d endz = new OdGePoint3d(start);
        endz.z += lineLength;
        CreateWcsAxis(wcsZ, new OdTvColorDef(20, 57, 245), start, endz, "Z");

        _wcsViewId.openObject().device().openObject().invalidate();
        _mm.StopTransaction(mtr);
    }

    public bool IsNeedUpdateWcs(OdTvGsView_RenderMode oldmode, OdTvGsView_RenderMode newmode)
    {
        MemoryTransaction mtr = _mm.StartTransaction();
        OdTvGsView wcsView = _wcsViewId.openObject(OdTv_OpenMode.kForWrite);
        if (wcsView == null)
        {
            _mm.StopTransaction(mtr);
            return false;
        }

        bool bOldModeWire = false;
        if (oldmode == OdTvGsView_RenderMode.k2DOptimized || oldmode == OdTvGsView_RenderMode.kWireframe)
            bOldModeWire = true;

        bool bNewModeWire = false;
        if (newmode == OdTvGsView_RenderMode.k2DOptimized || newmode == OdTvGsView_RenderMode.kWireframe)
            bNewModeWire = true;

        wcsView.setMode(bNewModeWire ? OdTvGsView_RenderMode.kWireframe : OdTvGsView_RenderMode.kGouraudShaded);

        if (bOldModeWire != bNewModeWire)
        {
            _mm.StopTransaction(mtr);
            return true;
        }

        _mm.StopTransaction(mtr);
        return false;
    }

    private void CreateWcsAxis(OdTvGeometryDataId wcsId, OdTvColorDef color, OdGePoint3d startPoint, OdGePoint3d endPoint, string axisName)
    {
        MemoryTransaction mtr = _mm.StartTransaction();

        OdTvEntity pWcs = wcsId.openAsSubEntity(OdTv_OpenMode.kForWrite);
        pWcs.setColor(color);

        OdTvGsView view = _wcsViewId.openObject();

        OdGePoint3d labelRefPoint = new OdGePoint3d(endPoint);

        // draw lines in wireframe and draw cylinders in shaded modes
        if ((int)view.mode() == (int)OdGsView_RenderMode.k2DOptimized || (int)view.mode() == (int)OdGsView_RenderMode.kWireframe)
        {
            //append axis
            pWcs.appendPolyline(startPoint, endPoint);
        }
        else
        {
            // distance to the end of the arrow
            double lastPointDist = 0.022;
            OdGePoint3d lastPoint = new OdGePoint3d(endPoint);
            if (axisName == "X")
                lastPoint.x += lastPointDist;
            else if (axisName == "Y")
                lastPoint.y += lastPointDist;
            else if (axisName == "Z")
                lastPoint.z += lastPointDist;

            OdGePoint3dVector pnts = new OdGePoint3dVector();
            pnts.Add(new OdGePoint3d(startPoint));
            pnts.Add(new OdGePoint3d(endPoint));
            pnts.Add(new OdGePoint3d(endPoint));
            pnts.Add(new OdGePoint3d(lastPoint));

            OdDoubleArray radii = new OdDoubleArray(4);
            radii.Add(0.007);
            radii.Add(0.007);
            radii.Add(0.015);
            radii.Add(0d);

            pWcs.appendShellFromCylinder(pnts, radii, OdTvCylinderData_Capping.kBoth, 50);

            // update label reference point
            labelRefPoint = lastPoint;
        }

        // append labels
        if (axisName == "X")
            labelRefPoint.x += 0.015;
        else if (axisName == "Y")
            labelRefPoint.y += 0.015;
        else if (axisName == "Z")
            labelRefPoint.z += 0.015;

        OdTvEntityId wcsTextEntityId = _tvWcsModelId.openObject(OdTv_OpenMode.kForWrite).appendEntity("TextEntity");
        OdTvEntity textEntity = wcsTextEntityId.openObject(OdTv_OpenMode.kForWrite);
        textEntity.setColor(color);
        textEntity.setAutoRegen(true);

        OdTvGeometryDataId labelTextId = textEntity.appendText(labelRefPoint, axisName);
        OdTvTextData labelText = labelTextId.openAsText();
        labelText.setAlignmentMode(OdTvTextStyle_AlignmentType.kMiddleCenter);
        labelText.setTextSize(0.02);
        labelText.setNonRotatable(true);

        if ((int)view.mode() != (int)OdGsView_RenderMode.k2DOptimized && (int)view.mode() != (int)OdGsView_RenderMode.kWireframe)
            textEntity.setLineWeight(new OdTvLineWeightDef(4));

        _mm.StopTransaction(mtr);
    }

    public void RemoveWcs()
    {
        MemoryTransaction mtr = _mm.StartTransaction();
        // remove old wcs entities
        _tvWcsModelId.openObject(OdTv_OpenMode.kForWrite).clearEntities();
        _mm.StopTransaction(mtr);
    }

    public OdTvGsViewId GetWcsViewId()
    {
        return _wcsViewId;
    }

    public OdTvGsViewId GetParentViewId()
    {
        return _activeViewId;
    }

    public OdTvGsView GetWcsView(OdTv_OpenMode mode)
    {
        return _wcsViewId.openObject(mode);
    }

    public OdTvGsView GetParentView(OdTv_OpenMode mode)
    {
        return _activeViewId.openObject(mode);
    }
}

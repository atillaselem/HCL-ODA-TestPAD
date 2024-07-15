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
using ODA.Visualize.TV_Visualize;

namespace HCL_ODA_TestPAD.ODA.Draggers.Construct;

public class OdTvPolylineDragger : OdTvBaseConstructDragger
{
    public OdTvPolylineDragger(OdTvGsDeviceId tvDeviceId, OdTvModelId tvDraggersModelId, OdTvModelId activeModel)
        : base(tvDeviceId, tvDraggersModelId, activeModel)
    {
        _bDraggerShouldBeRestarted = true;
    }

    public override void UpdateGeometry(bool bCreate, bool bFromNextPoint)
    {
        OdTvGsView pView = TvView.openObject();
        if (pView == null)
            return;

        //update or create entity
        if (bCreate)
        {
            OdTvModel modelPtr = TvDraggerModelId.openObject(OdTv_OpenMode.kForWrite);
            _entityId = modelPtr.appendEntity();
            {
                OdTvEntity entityNewPtr = _entityId.openObject(OdTv_OpenMode.kForWrite);
                entityNewPtr.setColor(TvDraggerColor);
                //create line
                _newGeometryId = entityNewPtr.appendPolyline(_clickedPts[0], _clickedPts[1]);
                entityNewPtr.Dispose();
            }
            modelPtr.Dispose();
        }
        else
        {
            OdTvGeometryData geometryPtr = _newGeometryId.openObject();
            if (geometryPtr == null || geometryPtr.getType() != OdTv_OdTvGeometryDataType.kPolyline)
            {
                pView.Dispose();
                geometryPtr.Dispose();
                return;
            }

            OdTvPolylineData linePtr = geometryPtr.getAsPolyline();
            if (linePtr != null)
            {
                linePtr.setPoints(_clickedPts[0], _clickedPts[1]);
                linePtr.Dispose();
            }
            geometryPtr.Dispose();
        }
    }
}
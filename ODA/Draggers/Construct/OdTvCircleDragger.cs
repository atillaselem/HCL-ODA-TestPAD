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
using Teigha.Core;
using Teigha.Visualize;

namespace HCL_ODA_TestPAD.ODA.Draggers.Construct;

public class OdTvCircleDragger : OdTvBaseConstructDragger
{
    public OdTvCircleDragger(OdTvGsDeviceId tvDeviceId, OdTvModelId tvDraggersModelId, OdTvModelId activeModel)
        : base(tvDeviceId, tvDraggersModelId, activeModel)
    {

    }

    public override void UpdateGeometry(bool bCreate, bool bFromNextPoint)
    {
        OdTvGsView pView = TvView.openObject();
        if (pView == null)
            return;

        OdGeVector3d radius = _clickedPts[1] - _clickedPts[0];
        double circleRadius = radius.length();

        //update or create entity
        if (bCreate)
        {
            OdTvModel modelPtr = TvDraggerModelId.openObject(OpenMode.kForWrite);
            _entityId = modelPtr.appendEntity();
            {
                OdTvEntity entityNewPtr = _entityId.openObject(OpenMode.kForWrite);
                entityNewPtr.setColor(TvDraggerColor);
                //create circle
                _newGeometryId = entityNewPtr.appendCircle(_clickedPts[0], circleRadius, OdGeVector3d.kZAxis);
                entityNewPtr.Dispose();
            }
            modelPtr.Dispose();
        }
        else
        {
            OdTvGeometryData geometryPtr = _newGeometryId.openObject();
            if (geometryPtr == null || geometryPtr.getType() != OdTvGeometryDataType.kCircle)
            {
                pView.Dispose();
                geometryPtr.Dispose();
                return;
            }

            OdTvCircleData circlePtr = geometryPtr.getAsCircle();
            if (circlePtr != null)
            {
                circlePtr.setRadius(circleRadius);
                circlePtr.Dispose();
            }
            geometryPtr.Dispose();
        }

        pView.Dispose();
    }
}
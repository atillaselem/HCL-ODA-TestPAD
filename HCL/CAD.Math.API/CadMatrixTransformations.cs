// Copyright © 2018 by Hilti Corporation – all rights reserved

using HCL_ODA_TestPAD.HCL.Visualize.Extensions;
using ODA.Visualize.TV_Visualize;

namespace HCL_ODA_TestPAD.HCL.CAD.Math.API
{
    internal static class CadMatrixTransformations
    {
        internal static CadMatrix3D EyeToWorldMatrix(this OdTvGsViewId @this)
        {
            using var view = @this.GsView();

            return view?.eyeToWorldMatrix() ?? CadMatrix3D.Identity;
        }

        internal static CadMatrix3D WorldToEyeMatrix(this OdTvGsViewId @this)
        {
            using var view = @this.GsView();

            return view?.viewingMatrix() ?? CadMatrix3D.Identity;
        }

        internal static CadMatrix3D WorldToDeviceMatrix(this OdTvGsViewId @this)
        {
            using var view = @this.GsView();

            return view?.worldToDeviceMatrix() ?? CadMatrix3D.Identity;
        }

        internal static CadMatrix3D ScreenToCameraMatrix(this OdTvGsViewId @this)
        {
            using var view = @this.GsView();
            using var screenMatrix = view?.screenMatrix();
            using var projectionMatrix = view?.projectionMatrix();
            //Camera => Screen
            using var onScreenProjectedMatrix = screenMatrix * projectionMatrix;
            //Screen => Camera
            return onScreenProjectedMatrix.inverse();
        }

        internal static void Pan(this OdTvGsViewId @this, CadVector3D vector)
        {
            using var cameraMatrix = @this.ScreenToCameraMatrix();
            using var _ = vector.TransformWith(cameraMatrix);
            Dolly(@this, vector);
        }

        internal static void Dolly(this OdTvGsViewId @this, CadVector3D vector)
        {
            using var view = @this.GsView();
            view?.dolly(vector);
        }
    }
}

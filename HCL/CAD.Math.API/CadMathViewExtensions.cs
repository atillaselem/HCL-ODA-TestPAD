// Copyright © 2018 by Hilti Corporation – all rights reserved
#nullable enable
using HCL_ODA_TestPAD.HCL.Visualize;
using HCL_ODA_TestPAD.HCL.Visualize.Extensions;
using ODA.Visualize.TV_Visualize;

namespace HCL_ODA_TestPAD.HCL.CAD.Math.API
{
    internal static class CadMathViewExtensions
    {
        internal static CadVector3D UpVector(this OdTvGsViewId @this)
        {
            using var view = @this.GsView();
            return view?.upVector() ?? CadVector3D.Default;
        }
        internal static CadPoint3D Target(this OdTvGsViewId @this)
        {
            using var view = @this.GsView();
            return view?.target() ?? CadPoint3D.Default;
        }
        internal static CadPoint3D Position(this OdTvGsViewId @this)
        {
            using var view = @this.GsView();
            return view?.position() ?? CadPoint3D.Default;
        }
        internal static CadVector3D Direction(this OdTvGsViewId @this)
        {
            using var direction = DirectionVector(@this);
            return direction.Normalize();
        }

        private static CadVector3D DirectionVector(this OdTvGsViewId @this)
        {
            using var position = Position(@this);
            using var target = Target(@this);
            return position - target;
        }
        internal static double FieldWidth(this OdTvGsViewId @this)
        {
            using var view = @this.GsView();
            return view.fieldWidth();
        }

        internal static double FieldHeight(this OdTvGsViewId @this)
        {
            using var view = @this.GsView();
            return view.fieldHeight();
        }
        internal static bool IsInInteractivity(this OdTvGsViewId @this)
        {
            using var view = @this.GsView();
            return view.isInInteractivity();
        }
        internal static bool IsPerspective(this OdTvGsViewId @this)
        {
            using var view = @this.GsView();
            return view.isPerspective();
        }
        internal static void SetView(this OdTvGsViewId @this, CadPoint3D position, CadPoint3D target, CadVector3D axis, double fieldWidth,
                    double fieldHeight,
                    ProjectionTypes projectionType)
        {
            using var view = @this.GsView(OdTv_OpenMode.kForWrite);

            switch (projectionType)
            {
                case ProjectionTypes.Parallel:
                    view.setView(position, target, axis, fieldWidth, fieldHeight, OdTvGsView_Projection.kParallel);

                    break;
                case ProjectionTypes.Perspective:
                    view.setView(position, target, axis, fieldWidth, fieldHeight, OdTvGsView_Projection.kPerspective);

                    break;
            }
        }
    }
}

using System;
using HCL_ODA_TestPAD.HCL.CAD.Math.API;
using ODA.Visualize.TV_Visualize;

namespace HCL_ODA_TestPAD.HCL.Visualize.Extensions
{
    internal static class OdTvGsViewExtensions
    {
        internal static OdTvGsView GsView(this OdTvGsViewId @this, OdTv_OpenMode openMode = OdTv_OpenMode.kForRead)
            => @this.openObject(openMode);

        public static CadPoint2D GetPixelCalculationPoint(this OdTvGsViewId @this)
        {
            var pixCalcPoint = CadPoint2D.Default;
            using var target = @this.Target();
            using var odTvGsView = @this.openObject(OdTv_OpenMode.kForRead);
            odTvGsView.getNumPixelsInUnitSquare(target, pixCalcPoint);
            return pixCalcPoint;
        }

        public static double GetPixelScaleFactorAtViewTarget(this OdTvGsViewId @this, double scale = 1)
        {
            using var pixCalcPoint = CadPoint2D.Default;
            using var target = @this.Target();
            using var odTvGsView = @this.openObject(OdTv_OpenMode.kForRead);
            odTvGsView.getNumPixelsInUnitSquare(target, pixCalcPoint);
            return scale / pixCalcPoint.X;
        }

        internal static CadPoint2D GetPixelDensityAtWcsPoint3d(this OdTvGsViewId @this, CadPoint3D target,
            CadPoint2D pixCalcPoint)
        {
            using var odTvGsView = @this.openObject(OdTv_OpenMode.kForRead);
            odTvGsView.getNumPixelsInUnitSquare(target, pixCalcPoint);
            if (pixCalcPoint.X < CadModelConstants.MinScaleCoef)
            {
                return pixCalcPoint with { X = 1 };
            }

            return pixCalcPoint with { };
        }

        internal static bool IsCADRotated(this OdTvGsViewId @this)
        {
            using var upVector = @this.UpVector();
            return !(upVector.X == 0 || upVector.Y == 0 || upVector.Z == 0);
        }
        private static bool IsCADInTopView(this OdTvGsViewId @this)
        {
            using var upVector = @this.UpVector();
            var z = Math.Round(upVector.Z, 1);
            return z == 0;
        }
        public static double GetPixelScaleAtEyeSystem(this OdTvGsViewId @this, CadPoint3D midPoint, double scale = 1)
        {
            using var pointDimension = CadPoint2D.Default;
            using var pixelDensityAtWcs = @this.GetPixelDensityAtWcsPoint3d(midPoint, pointDimension);
            using var viewMatrix = @this.WorldToEyeMatrix();
            using var pixelVectorAtWcs = CadVector3D.With(scale / pixelDensityAtWcs.X, 0, 0);
            using var pixelVectorAtEyeSystem = pixelVectorAtWcs.TransformWith(viewMatrix);
            var pixelScaleAtEyeSystem = pixelVectorAtEyeSystem.Length();

            // Creates a constant sized object on the map
            if (pixelScaleAtEyeSystem < CadModelConstants.MinScaleCoef)
            {
                pixelScaleAtEyeSystem = CadModelConstants.MinScaleCoef;
            }
            return pixelScaleAtEyeSystem;
        }
    }
}

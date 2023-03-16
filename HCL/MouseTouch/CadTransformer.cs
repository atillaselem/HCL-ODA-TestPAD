using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HCL_ODA_TestPAD.HCL.CadUnits;
using Teigha.Core;
using Teigha.Visualize;

namespace HCL_ODA_TestPAD.HCL.MouseTouch
{
    public sealed class CadTransformer : ICadTransformation
    {
        private readonly OdTvGsViewId _odTvGsViewId;
        public CadTransformer(OdTvGsViewId odTvGsViewId)
        {
            _odTvGsViewId = odTvGsViewId;
        }
        /// <summary>
        /// Converts the given screen coordinates of the window to world coordinates.
        /// 2D screen coordinate to 3D world space coordinate.
        /// </summary>
        /// <returns>operation result</returns>
        public OdGePoint3d GetWorldCoordinates(double x, double y)
        {
            var wcsPoint = new OdGePoint3d();
            wcsPoint.set(x, y, 0.0);
            Transform(wcsPoint);
            return wcsPoint;
        }

        //World To Screen Transformation point object
        private readonly OdGePoint3d _wcsPt = new(0, 0, 0);
        /// <summary>
        /// Transform World Coordinate To Screen Coordinate
        /// </summary>
        public Point TransformWorldToScreen(double x, double y)
        {
            using var odTvGsView = _odTvGsViewId.openObject(OpenMode.kForWrite);
            _wcsPt.set(UnitConverter.MetersToMapUnits(x), UnitConverter.MetersToMapUnits(y), 0);
            _wcsPt.transformBy(odTvGsView.worldToDeviceMatrix());
            return new Point(_wcsPt.x, _wcsPt.y);
        }

        /// <summary>
        /// Converts the screen coordinate(x,y) to world coordinate(x,y,z).
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>operation result</returns>
        private void Transform(OdGePoint3d point)
        {
            if (point == null)
            {
                return;
            }

            using var odTvGsView = _odTvGsViewId.openObject(OpenMode.kForWrite);
            point.transformBy((odTvGsView.screenMatrix() * odTvGsView.projectionMatrix()).inverse());
            point.z = 0.0;
            point.transformBy(odTvGsView.eyeToWorldMatrix());
        }

        public OdGePoint3d ToEyeToWorld(int x, int y)
        {
            using var odTvGsView = _odTvGsViewId.openObject(OpenMode.kForWrite);
            var wcsPt = new OdGePoint3d(x, y, 0.0);
            wcsPt = wcsPt.transformBy((odTvGsView.screenMatrix() * odTvGsView.projectionMatrix()).inverse());
            wcsPt = new OdGePoint3d(wcsPt.x, wcsPt.y, 0.0);
            return wcsPt.transformBy(odTvGsView.eyeToWorldMatrix());
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _isDisposed;

        /// <summary>
        ///  Disposes managed code if any
        /// </summary>
        /// <param name="disposing">Disposing flag</param>
        private void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                _wcsPt.Dispose();
            }

            _isDisposed = true;
        }
    }
}

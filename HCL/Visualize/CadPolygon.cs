using HCL_ODA_TestPAD.HCL.CAD.Math.API;
using System;

namespace HCL_ODA_TestPAD.HCL.Visualize
{
    public sealed class CadPolygon : CadEntity, IDisposable
    {
        private readonly CadPoint3D[] _points;

        public CadPolygon(CadPoint3D[] points)
        {
            _points = points;
        }

        public bool Filled { get; set; } = true;

        public bool FillingModeEverywhere { get; set; }

        public CadPoint3D[] GetPoints() => _points;

        public void Dispose()
        {
            foreach (var point in _points)
            {
                point?.Dispose();
            }
        }
    }
}

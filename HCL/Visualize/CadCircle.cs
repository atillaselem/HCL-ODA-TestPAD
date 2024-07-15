using HCL_ODA_TestPAD.HCL.CAD.Math.API;
using System;

namespace HCL_ODA_TestPAD.HCL.Visualize
{
    public sealed class CadCircle : CadEntity, IDisposable
    {
        public CadCircle(double radius, CadPoint3D centerLoc)
        {
            Radius = radius;
            CenterLoc = CadPoint3D.With(centerLoc);
        }

        public CadCircle(double radius, CadPoint3D centerLoc, string layer) : this(radius, centerLoc)
        {
            Layer = layer;
        }

        public CadPoint3D CenterLoc { internal set; get; }
        public CadVector3D Normal { internal set; get; }
        public double Radius { internal set; get; }
        public bool Filled { get; internal set; }

        public void Dispose()
        {
            CenterLoc?.Dispose();
            Normal?.Dispose();
        }
    }
}

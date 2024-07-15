using HCL_ODA_TestPAD.HCL.CAD.Math.API;

using ODA.Kernel.TD_RootIntegrated;
using System.Drawing;

namespace HCL_ODA_TestPAD.HCL.Visualize
{
    public enum LineWeightType
    {
        None = 0,
        Normal = 1,
        BoldS = 2,
        BoldM = 3,
        BoldL = 4,
        BoldXL = 6,
        BoldXXL = 10,
    }
    public enum ModelLifeCycle
    {
        None = 0,
        Created = 1,
        Hidden = 2,
        Visible = 3,
        Removed = 4
    }

    public abstract class CadEntity
    {
        //public static CadEntity CreateArc(CadPoint3D center, double radius)
        //{
        //    return new CadArc(radius: radius, centerLoc: center);
        //}

        public static CadEntity CreateCircle(CadPoint3D center, double radius)
        {
            return new CadCircle(radius: radius, centerLoc: center);
        }

        public static CadLine CreateLine(CadPoint3D start, CadPoint3D end)
        {
            return new CadLine(startLoc: start, endLoc: end);
        }

        public Color Color { get; set; } = Color.Red;

        public string Layer { get; set; }

        public double LineScale { get; set; }

        public bool IsSliceable { get; set; } = true;

        /// <summary>
        /// Ranges from 0 to 1, 1 being fully transparent 
        /// </summary>
        public double Transparency { get; set; }

        public LineWeightType LineWeight { get; set; }

        public OdPs_LineType LineStyle { get; set; }
    }
}

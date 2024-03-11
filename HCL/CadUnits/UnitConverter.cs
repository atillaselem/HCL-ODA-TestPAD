using System;
using ODA.Kernel.TD_RootIntegrated;

namespace HCL_ODA_TestPAD.HCL.CadUnits
{
    public static class UnitConverter
    {
        public static double MISSING_VALUE { get; } = -9.99E+27;
        public static double MapUnitsToMetersConversionFactor { get; set; } = 1.0;

        public static double MetersToMapUnitsConversionFactor { get; set; } = 1.0;

        public static double MapUnitsToMeters(double len)
        {
            double dConvFactor = MapUnitsToMetersConversionFactor;
            return len * dConvFactor;
        }

        public static OdGePoint3d MetersToMapUnits(OdGePoint3d loc)
        {
            if (loc == null)
            {
                throw new ArgumentNullException(nameof(loc));
            }

            double dConvFactor = MetersToMapUnitsConversionFactor;

            OdGePoint3d newLoc = new(loc.x * dConvFactor, loc.y * dConvFactor, loc.z * dConvFactor);

            if (loc.z == MISSING_VALUE)
            {
                newLoc.z = 0.0;
            }

            return newLoc;
        }

        public static double MetersToMapUnits(double len)
        {
            double dConvFactor = MetersToMapUnitsConversionFactor;
            return len * dConvFactor;
        }
    }
}

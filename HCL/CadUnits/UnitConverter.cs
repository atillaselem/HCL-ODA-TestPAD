using System;
using HCL_ODA_TestPAD.HCL.CAD.Math.API;
using ODA.Kernel.TD_RootIntegrated;

namespace HCL_ODA_TestPAD.HCL.CadUnits
{
    public static class UnitConverter
    {
        public static double MissingValue { get; } = -9.99E+27;
        public static double MapUnitsToMetersConversionFactor { get; set; } = 1.0;

        public static double MetersToMapUnitsConversionFactor { get; set; } = 1.0;

        public static double MapUnitsToMeters(double len)
        {
            var dConvFactor = MapUnitsToMetersConversionFactor;
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

            if (loc.z == MissingValue)
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

        public static HcLocation MapUnitsToMeters(HcLocation loc)
        {
            ArgumentNullException.ThrowIfNull(loc);

            double dConvFactor = MapUnitsToMetersConversionFactor;

            HcLocation newLoc = new(loc.Easting * dConvFactor, loc.Northing * dConvFactor, loc.Elevation * dConvFactor);

            return newLoc;
        }
    }
}

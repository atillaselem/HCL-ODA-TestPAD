using System;
using ODA.Kernel.TD_RootIntegrated;
using ODA.Visualize.TV_Visualize;

namespace HCL_ODA_TestPAD.HCL.CadUnits
{
    public enum SurveyUnits
    {
        Meters,
        Centimeters,
        Millimeters,
        Inches,
        Inches116,
        Usfeet,
        Feet,
        FeetInch18,
        FeetInch116,
        MaxUnits
    }
    internal static class UnitsValueConverter
    {
        /// <summary>
        /// Maps Survey units to Cad units.
        /// </summary>
        /// <param name="surveyUnit">The survey unit.</param>
        /// <param name="isMetric">if set to <c>true</c> [is metric].</param>
        /// <returns>UnitsValue.</returns>
        public static UnitsValue ConvertUnits(SurveyUnits surveyUnit, out bool isMetric)
        {
            isMetric = true;
            UnitsValue units = UnitsValue.kUnitsMeters;

            switch (surveyUnit)
            {
                case SurveyUnits.Inches:
                    {
                        isMetric = false;
                        units = UnitsValue.kUnitsInches;
                        break;
                    }
                case SurveyUnits.Feet:
                case SurveyUnits.FeetInch18:
                case SurveyUnits.FeetInch116:
                    {
                        isMetric = false;
                        units = UnitsValue.kUnitsFeet;
                        break;
                    }
                case SurveyUnits.Usfeet:
                    {
                        isMetric = false;
                        units = UnitsValue.kUnitsUSSurveyFeet;
                        break;
                    }
                case SurveyUnits.Meters:
                    {
                        units = UnitsValue.kUnitsMeters;
                        break;
                    }
                case SurveyUnits.Millimeters:
                    {
                        units = UnitsValue.kUnitsMillimeters;
                        break;
                    }
                case SurveyUnits.Centimeters:
                    {
                        units = UnitsValue.kUnitsCentimeters;
                        break;
                    }
            }
            return units;
        }

        /// <summary>
        /// Maps the units to meters conversion.
        /// </summary>
        /// <param name="eUnits">The e units.</param>
        /// <returns>System.Double.</returns>
        public static double MapUnitsToMetersConversion(UnitsValue eUnits)
        {
            switch (eUnits)
            {
                case UnitsValue.kUnitsInches:
                    return CadModelConstants.Inches2Meters;

                case UnitsValue.kUnitsFeet:
                    return CadModelConstants.Feet2Meters;

                case UnitsValue.kUnitsUSSurveyFeet:
                    return CadModelConstants.UsFeet2Meter;

                case UnitsValue.kUnitsMillimeters:
                    return CadModelConstants.Millimeters2Meters;

                case UnitsValue.kUnitsCentimeters:
                    return CadModelConstants.Centimeters2Meters;

                case UnitsValue.kUnitsMeters:
                    return CadModelConstants.DefaultMeters;

                default:
                    throw new ArgumentOutOfRangeException(nameof(eUnits), "Units not supported");
            }
        }

        /// <summary>
        /// Meters to map units conversion.
        /// </summary>
        /// <param name="eUnits">The e units.</param>
        /// <returns>System.Double.</returns>
        public static double MetersToMapUnitsConversion(UnitsValue eUnits)
        {
            switch (eUnits)
            {
                case UnitsValue.kUnitsInches:
                    return CadModelConstants.Meters2Inches;

                case UnitsValue.kUnitsFeet:
                    return CadModelConstants.Meters2Feet;

                case UnitsValue.kUnitsUSSurveyFeet:
                    return CadModelConstants.Meter2UsFeet;

                case UnitsValue.kUnitsMillimeters:
                    return CadModelConstants.Meters2Millimeters;

                case UnitsValue.kUnitsCentimeters:
                    return CadModelConstants.Meters2Centimeters;

                case UnitsValue.kUnitsMeters:
                    return CadModelConstants.DefaultMeters;

                default:
                    throw new ArgumentOutOfRangeException(nameof(eUnits), "Units not supported");
            }
        }

        /// <summary>
        /// Gets the scale value.
        /// </summary>
        /// <param name="fromUnits">From units.</param>
        /// <param name="toUnits">To units.</param>
        /// <returns>System.Double.</returns>
        public static double GetScaleValue(UnitsValue fromUnits, UnitsValue toUnits)
        {
            //Initially, converts from units to meters
            double scaleValue = MapUnitsToMetersConversion(fromUnits);

            //Convert from meters to user selected units
            return ConvertMeterstoSelectedUnits(toUnits, scaleValue);
        }

        /// <summary>
        /// Gets the point size for export.
        /// </summary>
        /// <param name="toUnits">To units.</param>
        /// <returns>System.Double.</returns>
        public static double GetPointSize(UnitsValue toUnits)
        {
            //Convert from meters to user selected units
            return ConvertMeterstoSelectedUnits(toUnits, CadModelConstants.PointSizeDwgExportMeter);
        }

        /// <summary>
        /// converts the meter value
        /// </summary>
        /// <param name="toUnits">To Units</param>
        /// <param name="meterValue">Value in meters</param>
        /// <returns>System.Double</returns>
        private static double ConvertMeterstoSelectedUnits(UnitsValue toUnits, double meterValue)
        {
            switch (toUnits)
            {
                case UnitsValue.kUnitsInches:
                    return meterValue * CadModelConstants.Meters2Inches;

                case UnitsValue.kUnitsUSSurveyFeet:
                    return meterValue * CadModelConstants.Meter2UsFeet;

                case UnitsValue.kUnitsFeet:
                    return meterValue * CadModelConstants.Meters2Feet;

                case UnitsValue.kUnitsMillimeters:
                    return meterValue * CadModelConstants.Meters2Millimeters;

                case UnitsValue.kUnitsCentimeters:
                    return meterValue * CadModelConstants.Meters2Centimeters;

                case UnitsValue.kUnitsMeters:
                    return meterValue;

                default:
                    throw new ArgumentOutOfRangeException(nameof(toUnits), "Units not supported");
            }
        }

        public static UnitsValue MapOdaUnitsToHilti(OdTv_Units modelUnits, double userDefCoef)
        {
            //Mapping Visulize Units to Drawings UnitsValue
            return modelUnits switch
            {
                OdTv_Units.kUserDefined => userDefCoef == CadModelConstants.UsFeetCoefValue ? UnitsValue.kUnitsUSSurveyFeet : UnitsValue.kUnitsUndefined,
                OdTv_Units.kMeters => UnitsValue.kUnitsMeters,
                OdTv_Units.kCentimeters => UnitsValue.kUnitsCentimeters,
                OdTv_Units.kMillimeters => UnitsValue.kUnitsMillimeters,
                OdTv_Units.kFeet => UnitsValue.kUnitsFeet,
                OdTv_Units.kInches => UnitsValue.kUnitsInches,
                _ => UnitsValue.kUnitsUndefined,
            };
        }

        public static OdTv_Units MapHiltiUnitsToOda(UnitsValue unitsValue)
        {
            //Mapping Drawings UnitsValue to Visulize Units
            return unitsValue switch
            {
                UnitsValue.kUnitsMeters => OdTv_Units.kMeters,
                UnitsValue.kUnitsCentimeters => OdTv_Units.kCentimeters,
                UnitsValue.kUnitsMillimeters => OdTv_Units.kMillimeters,
                UnitsValue.kUnitsFeet => OdTv_Units.kFeet,
                UnitsValue.kUnitsInches => OdTv_Units.kInches,
                UnitsValue.kUnitsUSSurveyFeet => OdTv_Units.kUserDefined,
                _ => OdTv_Units.kMeters,
            };
        }

        public static SurveyUnits MapOdaUnitsToSurveyUnits(OdTv_Units units, double userDefCoef)
        {
            //Mapping Cad File Unit read from file to Survey Units shown in Settings Combobox
            return units switch
            {
                OdTv_Units.kMeters => SurveyUnits.Meters,
                OdTv_Units.kCentimeters => SurveyUnits.Centimeters,
                OdTv_Units.kMillimeters => SurveyUnits.Millimeters,
                OdTv_Units.kFeet => SurveyUnits.Feet,
                OdTv_Units.kInches => SurveyUnits.Inches,
                OdTv_Units.kUserDefined => userDefCoef == CadModelConstants.UsFeetCoefValue ? SurveyUnits.Usfeet : SurveyUnits.Meters,
                _ => SurveyUnits.Meters,
            };
        }
    }
}

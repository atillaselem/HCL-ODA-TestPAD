using System;
using Teigha.Core;
using Teigha.Visualize;

namespace HCL_ODA_TestPAD.HCL.CadUnits
{
    public enum SurveyUnits
    {
        meters,
        centimeters,
        millimeters,
        inches,
        inches_1_16,
        usfeet,
        feet,
        feet_inch_1_8,
        feet_inch_1_16,
        max_units
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
                case SurveyUnits.inches:
                    {
                        isMetric = false;
                        units = UnitsValue.kUnitsInches;
                        break;
                    }
                case SurveyUnits.feet:
                case SurveyUnits.feet_inch_1_8:
                case SurveyUnits.feet_inch_1_16:
                    {
                        isMetric = false;
                        units = UnitsValue.kUnitsFeet;
                        break;
                    }
                case SurveyUnits.usfeet:
                    {
                        isMetric = false;
                        units = UnitsValue.kUnitsUSSurveyFeet;
                        break;
                    }
                case SurveyUnits.meters:
                    {
                        units = UnitsValue.kUnitsMeters;
                        break;
                    }
                case SurveyUnits.millimeters:
                    {
                        units = UnitsValue.kUnitsMillimeters;
                        break;
                    }
                case SurveyUnits.centimeters:
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
                    return CADModelConstants.Inches2Meters;

                case UnitsValue.kUnitsFeet:
                    return CADModelConstants.Feet2Meters;

                case UnitsValue.kUnitsUSSurveyFeet:
                    return CADModelConstants.UsFeet2Meter;

                case UnitsValue.kUnitsMillimeters:
                    return CADModelConstants.Millimeters2Meters;

                case UnitsValue.kUnitsCentimeters:
                    return CADModelConstants.Centimeters2Meters;

                case UnitsValue.kUnitsMeters:
                    return CADModelConstants.DefaultMeters;

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
                    return CADModelConstants.Meters2Inches;

                case UnitsValue.kUnitsFeet:
                    return CADModelConstants.Meters2Feet;

                case UnitsValue.kUnitsUSSurveyFeet:
                    return CADModelConstants.Meter2UsFeet;

                case UnitsValue.kUnitsMillimeters:
                    return CADModelConstants.Meters2Millimeters;

                case UnitsValue.kUnitsCentimeters:
                    return CADModelConstants.Meters2Centimeters;

                case UnitsValue.kUnitsMeters:
                    return CADModelConstants.DefaultMeters;

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
            return ConvertMeterstoSelectedUnits(toUnits, CADModelConstants.PointSizeDwgExportMeter);
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
                    return meterValue * CADModelConstants.Meters2Inches;

                case UnitsValue.kUnitsUSSurveyFeet:
                    return meterValue * CADModelConstants.Meter2UsFeet;

                case UnitsValue.kUnitsFeet:
                    return meterValue * CADModelConstants.Meters2Feet;

                case UnitsValue.kUnitsMillimeters:
                    return meterValue * CADModelConstants.Meters2Millimeters;

                case UnitsValue.kUnitsCentimeters:
                    return meterValue * CADModelConstants.Meters2Centimeters;

                case UnitsValue.kUnitsMeters:
                    return meterValue;

                default:
                    throw new ArgumentOutOfRangeException(nameof(toUnits), "Units not supported");
            }
        }

        public static UnitsValue MapOdaUnitsToHilti(Units modelUnits, double userDefCoef)
        {
            //Mapping Visulize Units to Drawings UnitsValue
            return modelUnits switch
            {
                Units.kUserDefined => userDefCoef == CADModelConstants.UsFeetCoefValue ? UnitsValue.kUnitsUSSurveyFeet : UnitsValue.kUnitsUndefined,
                Units.kMeters => UnitsValue.kUnitsMeters,
                Units.kCentimeters => UnitsValue.kUnitsCentimeters,
                Units.kMillimeters => UnitsValue.kUnitsMillimeters,
                Units.kFeet => UnitsValue.kUnitsFeet,
                Units.kInches => UnitsValue.kUnitsInches,
                _ => UnitsValue.kUnitsUndefined,
            };
        }

        public static Units MapHiltiUnitsToOda(UnitsValue unitsValue)
        {
            //Mapping Drawings UnitsValue to Visulize Units
            return unitsValue switch
            {
                UnitsValue.kUnitsMeters => Units.kMeters,
                UnitsValue.kUnitsCentimeters => Units.kCentimeters,
                UnitsValue.kUnitsMillimeters => Units.kMillimeters,
                UnitsValue.kUnitsFeet => Units.kFeet,
                UnitsValue.kUnitsInches => Units.kInches,
                UnitsValue.kUnitsUSSurveyFeet => Units.kUserDefined,
                _ => Units.kMeters,
            };
        }
    }
}

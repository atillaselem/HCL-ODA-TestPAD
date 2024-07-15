using System;
using static System.Math;

namespace HCL_ODA_TestPAD.HCL.CAD.Math.API
{
    public static class HcMath
    {
        public static int Sign(double x) => x >= 0.0 ? 1 : -1;

        public static bool IsValueInRange(double value, double min, double max)
        {
            if (min > max)
                throw new ArgumentException("min must be smaller than max", nameof(min));
            return value >= min && value <= max;
        }

        public static bool IsMissingValue(double dValue) => dValue < -9.99E+17;

        public static bool IsZeroDistance(double distance) => Abs(distance) < 1E-08;

        public static bool IsZeroAngle(double angle) => Abs(angle) < 4.84813681109536E-06;

        public static double Mod2Pi(double alpha) => HcMath.ModuloAngle(2.0 * PI, alpha);

        public static double SmallestAngleDiffInRad(double rads)
        {
            double num = HcMath.Mod2Pi(rads);
            if (num > PI)
                num -= 2.0 * PI;
            return num;
        }

        public static double Mod360(double x) => HcMath.ModuloAngle(360.0, x);

        public static double Mod400(double x) => HcMath.ModuloAngle(400.0, x);

        public static double ModuloAngle(double modulo, double alpha)
        {
            if (HcMath.IsMissingValue(alpha))
                return alpha;
            alpha %= modulo;
            if (alpha < 0.0)
                alpha += modulo;
            return alpha;
        }

        public static int SmallestModuloDiff(int modulo, int difference)
        {
            difference = HcMath.Modulo(modulo, difference);
            if (difference > modulo / 2)
                difference -= modulo;
            return difference;
        }

        public static int Modulo(int modulo, int value)
        {
            if (modulo <= 0)
                throw new ArgumentException("modulo must be strictly positive", nameof(modulo));
            value %= modulo;
            if (value < 0)
                value += modulo;
            return value;
        }

        public static double Deg2Rad(double x) => x * PI / 180.0;

        public static double Sec2Rad(double seconds) => HcMath.Deg2Rad(seconds / 3600.0);

        public static void SplitDecimal(double original, out double integral, out double fractional)
        {
            integral = original < 0.0 ? Ceiling(original) : Floor(original);
            fractional = original - integral;
        }

        public static double Min2Rad(double minutes) => HcMath.Deg2Rad(minutes / 60.0);

        public static double Angle2Percentage(double angleInRads) => Tan(angleInRads) * 100.0;
    }
}

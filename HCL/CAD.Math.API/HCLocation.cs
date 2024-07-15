#nullable enable
using static System.Math;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;

namespace HCL_ODA_TestPAD.HCL.CAD.Math.API
{
    public interface IDistance<in T>
    {
        double Distance2D(T t);
        double Distance3D(T t);
    }
    public class HcLocation : IDistance<HcLocation>
    {
        public static readonly HcLocation Origin = new HcLocation(0.0, 0.0, 0.0);
        public static readonly HcLocation Origin2D = new HcLocation(0.0, 0.0);
        public static readonly HcLocation Invalid = new HcLocation(HcConstants.MissingValue, HcConstants.MissingValue, HcConstants.MissingValue);

        public double Easting { get; }

        public double Northing { get; }

        public double Elevation { get; }

        public HcLocation(double easting, double northing, double elevation)
        {
            this.Easting = easting;
            this.Northing = northing;
            this.Elevation = elevation;
        }

        public HcLocation(double easting, double northing)
        {
            this.Easting = easting;
            this.Northing = northing;
            this.Elevation = HcConstants.MissingValue;
        }

        protected HcLocation(HcLocation location)
        {
            this.Easting = location.Easting;
            this.Northing = location.Northing;
            this.Elevation = location.Elevation;
        }

        public HcLocation Rotate(double angle)
        {
            double easting = this.Easting * Cos(angle) - this.Northing * Sin(angle);
            double northing = this.Easting * Sin(angle) + this.Northing * Cos(angle);
            return this.Is3D() ? new HcLocation(easting, northing, this.Elevation) : new HcLocation(easting, northing);
        }

        public HcLocation Rotate(double angle, HcLocation center) => this.Translate(-center.Easting, -center.Northing).Rotate(angle).Translate(center.Easting, center.Northing);

        public HcLocation SetElevation(double elevation) => new HcLocation(this.Easting, this.Northing, elevation);

        public double Distance3D(HcLocation t)
        {
            double num1 = this.Easting - t.Easting;
            double num2 = this.Northing - t.Northing;
            double num3 = 0.0;
            if (this.Is3D() && t.Is3D())
                num3 = this.Elevation - t.Elevation;
            return Sqrt(num1 * num1 + num2 * num2 + num3 * num3);
        }

        public double Distance2D(HcLocation t)
        {
            double num1 = this.Easting - t.Easting;
            double num2 = this.Northing - t.Northing;
            return Sqrt(num1 * num1 + num2 * num2);
        }

        public bool IsWithinDistance2D(HcLocation location, double radius) => this.Distance2D(location) < radius;

        public HcLocation Translate(double easting, double northing) => this.Translate3D(easting, northing, 0.0);

        public HcLocation Translate3D(double easting, double northing, double elevation) => new HcLocation(this.Easting + easting, this.Northing + northing, this.Elevation + elevation);

        public HcLocation Translate3D(Vector3D translationVector3d) => this.Translate3D(translationVector3d.X, translationVector3d.Y, translationVector3d.Z);

        public bool IsValid() => HcLocation.IsCoordinateValid(this.Easting) && HcLocation.IsCoordinateValid(this.Northing);

        public bool Is2D() => this.IsValid() && !this.IsElevationValid();

        public bool Is3D() => this.IsValid() && this.IsElevationValid();

        public bool IsElevationValid() => HcLocation.IsCoordinateValid(this.Elevation);

        public bool IsEqual2d(HcLocation loc) => HcMath.IsZeroDistance(this.Distance2D(loc));

        public bool IsEqual(HcLocation loc) => this.IsEqual2d(loc) && this.Is3D() == loc.Is3D() && HcMath.IsZeroDistance(this.Elevation - loc.Elevation);

        public override int GetHashCode() => (this.Northing.GetHashCode()) * 16777619 ^ this.Elevation.GetHashCode();

        public override bool Equals(object? obj) => obj is HcLocation loc && this.IsEqual(loc);

        private static bool IsCoordinateValid(double coordinate) => !HcMath.IsMissingValue(coordinate) && !double.IsNaN(coordinate);

        public void CornerAngle(
          HcLocation loc1,
          HcLocation loc2,
          out double leftAngle,
          out double rightAngle,
          out double oneEightyMinusRight)
        {
            double num1 = this.Bearing(loc1);
            double num2 = this.Bearing(loc2);
            rightAngle = HcMath.Mod2Pi(num2 - num1);
            leftAngle = 2.0 * PI - rightAngle;
            oneEightyMinusRight = HcMath.Mod2Pi(PI - rightAngle);
        }

        public double Bearing(HcLocation loc) => HcMath.Mod2Pi(new HcLine2D(this, loc).Bearing);

        public HcLocation ComputeLocation(HcLineShift shift)
        {
            if (!this.IsValid())
                return HcLocation.Invalid;
            Vector2D vector2D1 = new Vector2D(Sin(shift.Rotation), Cos(shift.Rotation));
            Vector2D vec1 = vector2D1.Scale(shift.Line);
            Vector2D vec2 = vector2D1.Orthogonal().Scale(shift.Offset);
            Vector2D vector2D2 = new Vector2D(this.Easting, this.Northing).Add(vec1).Add(vec2);
            return new HcLocation(vector2D2.X, vector2D2.Y, this.IsElevationValid() ? this.Elevation + shift.Height : this.Elevation);
        }

        public double SlopeDistance(HcLocation loc2) => !this.Is3D() || !loc2.Is3D() ? HcConstants.MissingValue : this.Distance3D(loc2);

        public Vector2D CreateVector2D(HcLocation head) => new Vector2D(head.Easting - this.Easting, head.Northing - this.Northing);

        public HcLocation Round(int digits)
        {
            if (!this.IsValid())
                return this;
            double easting = System.Math.Round(this.Easting, digits);
            double northing = System.Math.Round(this.Northing, digits);
            return this.Is2D() ? new HcLocation(easting, northing) : new HcLocation(easting, northing, System.Math.Round(this.Elevation, digits));
        }

        public static double ComputeLargestOneToOneDistance2D(IReadOnlyCollection<HcLocation> points)
        {
            double val2 = 0.0;
            for (int index1 = 0; index1 < points.Count - 1; ++index1)
            {
                HcLocation hcLocation = points.ElementAt<HcLocation>(index1);
                if (hcLocation.IsValid())
                {
                    for (int index2 = index1 + 1; index2 < points.Count; ++index2)
                    {
                        HcLocation t = points.ElementAt<HcLocation>(index2);
                        if (t.IsValid())
                            val2 = Max(hcLocation.Distance2D(t), val2);
                    }
                }
            }
            return val2;
        }
    }
}

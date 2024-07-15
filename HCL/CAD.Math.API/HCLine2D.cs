using static System.Math;

namespace HCL_ODA_TestPAD.HCL.CAD.Math.API
{
    public class HcLine2D
    {
        public HcLocation Point { get; }

        public double Bearing { get; }

        public bool IsValid { get; }

        public HcLine2D(HcLocation point, double bearing)
        {
            this.Point = point;
            this.IsValid = point.IsValid();
            this.Bearing = bearing;
        }

        public HcLine2D(HcLocation point1, HcLocation point2)
        {
            this.Point = point1;
            this.IsValid = point1.IsValid() && point2.IsValid() && point1.Distance2D(point2) > 1E-08;
            if (this.IsValid)
                this.Bearing = Atan2(point2.Easting - point1.Easting, point2.Northing - point1.Northing);
            else
                this.Bearing = HcConstants.MissingValue;
        }

        public HcLocation Intersect(HcLine2D line)
        {
            if (!this.IsValid || !line.IsValid)
                return HcLocation.Invalid;
            double num1 = Sin(this.Bearing);
            double num2 = Cos(this.Bearing);
            double num3 = Sin(line.Bearing);
            double num4 = Cos(line.Bearing);
            double num5 = num1 * num4 - num2 * num3;
            if (Abs(num5) < Pow(4.84813681109536E-06, 2.0))
                return HcLocation.Invalid;
            double num6 = line.Point.Easting - this.Point.Easting;
            double num7 = line.Point.Northing - this.Point.Northing;
            double num8 = (num6 * num4 - num7 * num3) / num5;
            return new HcLocation(this.Point.Easting + num8 * num1, this.Point.Northing + num8 * num2);
        }

        public HcLine2D CreateOrthogonal() => new HcLine2D(this.Point, this.Bearing + PI / 2.0);

        public HcLine2D Translate(double easting, double northing) => new HcLine2D(new HcLocation(this.Point.Easting + easting, this.Point.Northing + northing, this.Point.Elevation), this.Bearing);

        public HcLocation LocationOnLine(double line) => new HcLocation(this.Point.Easting + line * Sin(this.Bearing), this.Point.Northing + line * Cos(this.Bearing));
    }
}

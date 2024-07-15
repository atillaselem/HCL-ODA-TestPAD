using static System.Math;

namespace HCL_ODA_TestPAD.HCL.CAD.Math.API
{
    public class Vector2D
    {
        public Vector2D(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        public double X { get; }

        public double Y { get; }

        public double SignedAngle(Vector2D vec)
        {
            double num = Atan2(this.X * vec.Y - this.Y * vec.X, this.X * vec.X + this.Y * vec.Y);
            return num < 0.0 ? num + 2.0 * PI : num;
        }

        public double Dot(Vector2D vec) => vec.X * this.X + vec.Y * this.Y;

        public double CrossProduct(Vector2D that) => this.X * that.Y - this.Y * that.X;

        public double Length() => Sqrt(this.Dot(this));

        public Vector2D Add(Vector2D vec) => new Vector2D(this.X + vec.X, this.Y + vec.Y);

        public Vector2D Subtract(Vector2D vec) => this.Add(vec.Scale(-1.0));

        public Vector2D Scale(double factor) => new Vector2D(factor * this.X, factor * this.Y);

        public Vector2D Negate() => this.Scale(-1.0);

        public Vector2D Orthogonal() => new Vector2D(this.Y, -this.X);

        public Vector2D Normalize()
        {
            double distance = this.Length();
            return HcMath.IsZeroDistance(distance) ? new Vector2D(0.0, 0.0) : new Vector2D(this.X / distance, this.Y / distance);
        }
    }
}

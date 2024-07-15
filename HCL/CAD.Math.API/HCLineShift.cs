namespace HCL_ODA_TestPAD.HCL.CAD.Math.API
{
    public class HcLineShift
    {
        public static readonly HcLineShift ZeroShift = new HcLineShift(0.0, 0.0, 0.0, 0.0);
        public static readonly HcLineShift InvalidShift = new HcLineShift(HcConstants.MissingValue, HcConstants.MissingValue, HcConstants.MissingValue, HcConstants.MissingValue);

        public HcLineShift(double line, double offset, double height, double rotation)
        {
            this.Line = line;
            this.Offset = offset;
            this.Height = height;
            this.Rotation = rotation;
        }

        public double Line { get; }

        public double Offset { get; }

        public double Height { get; }

        public double Rotation { get; }

        public HcLineShift(double line, double offset, double height = 0.0)
            : this(line, offset, height, 0.0)
        {
        }

        public bool IsZero => (HcMath.IsZeroDistance(this.Line) || HcMath.IsMissingValue(this.Line)) && (HcMath.IsZeroDistance(this.Offset) || HcMath.IsMissingValue(this.Offset)) && (HcMath.IsZeroDistance(this.Height) || HcMath.IsMissingValue(this.Height)) && (HcMath.IsZeroAngle(this.Rotation) || HcMath.IsMissingValue(this.Rotation));

        public bool IsSame(HcLineShift otherShift) => HcMath.IsZeroDistance(otherShift.Line - this.Line) && HcMath.IsZeroDistance(otherShift.Offset - this.Offset) && HcMath.IsZeroDistance(otherShift.Height - this.Height) && HcMath.IsZeroAngle(otherShift.Rotation - this.Rotation);
    }
}

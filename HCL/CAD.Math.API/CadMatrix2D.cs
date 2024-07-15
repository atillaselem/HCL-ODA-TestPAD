// Copyright © 2018 by Hilti Corporation – all rights reserved
#pragma warning disable CA1062

using ODA.Kernel.TD_RootIntegrated;

namespace HCL_ODA_TestPAD.HCL.CAD.Math.API;

public record CadMatrix2D : CadMathValue<OdGeMatrix2d>
{
    private CadMatrix2D() { }
    private CadMatrix2D(OdGeMatrix2d p) : base(p.Mul(GeIdentity)) { }
    public static OdGeMatrix2d GeIdentity => new();
    public static CadMatrix2D Identity => new();

    public static implicit operator OdGeMatrix2d(CadMatrix2D p) => p.Value;
    public static implicit operator CadMatrix2D(OdGeMatrix2d p) => new(p);

    public double this[int index1, int index2] => Value[index1, index2];

    public static CadMatrix2D ScaleWith(double scale) => OdGeMatrix2d.scaling(scale);
    public static CadMatrix2D ScaleWith(double scaleX, double scaleY)
    {
        var matrix2D = GeIdentity;
        matrix2D[0, 0] = scaleX;
        matrix2D[1, 1] = scaleY;
        return matrix2D;
    }

    public static CadPoint2D operator *(CadMatrix2D matrix, CadPoint2D point) => matrix.Value * point.Value;

    public static CadVector2D operator *(CadMatrix2D matrix, CadVector2D vector) => matrix.Value * vector.Value;
}
#pragma warning restore CA1062
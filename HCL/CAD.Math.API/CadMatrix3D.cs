// Copyright © 2018 by Hilti Corporation – all rights reserved
#pragma warning disable CA1062

using ODA.Kernel.TD_RootIntegrated;

namespace HCL_ODA_TestPAD.HCL.CAD.Math.API;

public record CadMatrix3D : CadMathValue<OdGeMatrix3d>
{
    #region Constructors
    private CadMatrix3D() { }
    private CadMatrix3D(OdGeMatrix3d p) : base(p.Mul(GeIdentity)) { }
    #endregion

    #region Static Factories
    public static OdGeMatrix3d GeIdentity => new();
    public static CadMatrix3D Identity => new();
    public static CadMatrix3D ScaleWith(double scale) => OdGeMatrix3d.scaling(scale);
    public static CadMatrix3D ScaleWith(double scaleX, double scaleY, double scaleZ)
    {
        var matrix3D = GeIdentity;
        matrix3D[0, 0] = scaleX;
        matrix3D[1, 1] = scaleY;
        matrix3D[2, 2] = scaleZ;
        return matrix3D;
    }

    public static CadMatrix3D Translation(CadVector3D vec) => OdGeMatrix3d.translation(vec.Value);

    public static CadMatrix3D ScaleWithCenterPoint(double scale, CadPoint3D center) => OdGeMatrix3d.scaling(scale, center);

    public static CadMatrix3D AlignCoordSys(CadPoint3D fromOrigin,
                                            CadVector3D fromXAxis,
                                            CadVector3D fromYAxis,
                                            CadVector3D fromZAxis,
                                            CadPoint3D toOrigin,
                                            CadVector3D toXAxis,
                                            CadVector3D toYAxis,
                                            CadVector3D toZAxis) => OdGeMatrix3d.alignCoordSys(fromOrigin, fromXAxis, fromYAxis, fromZAxis, toOrigin, toXAxis, toYAxis, toZAxis);
    #endregion

    #region Implicit Operators
    public static implicit operator OdGeMatrix3d(CadMatrix3D p) => p.Value;
    public static implicit operator CadMatrix3D(OdGeMatrix3d p) => new(p);
    #endregion

    #region Overloaded Operators
    public double this[int index1, int index2] => Value[index1, index2];
    public static CadPoint3D operator *(CadMatrix3D matrix, CadPoint3D point) => matrix.Value * point.Value;
    public static CadVector3D operator *(CadMatrix3D matrix, CadVector3D vector) => matrix.Value * vector.Value;
    public static CadMatrix3D operator *(CadMatrix3D matrix1, CadMatrix3D matrix2) => matrix1.Value.Mul(matrix2.Value);
    #endregion

    #region Public Methods
    public CadMatrix3D ToIdentity() => Value.setToIdentity();
    public CadMatrix3D PreMultiplyWith(CadMatrix3D matrix) => Value.preMultBy(matrix);
    public CadMatrix3D PostMultiplyWith(CadMatrix3D matrix) => Value.postMultBy(matrix);
    public CadMatrix3D SetTranslation(CadVector3D vector) => Value.setTranslation(vector);
    public bool IsEqualTo(CadMatrix3D matrix) => Value.IsEqual(matrix.Value);
    public CadVector3D XAxis() => Value.getCsXAxis();
    public CadVector3D YAxis() => Value.getCsYAxis();
    public CadVector3D ZAxis() => Value.getCsZAxis();

    public CadMatrix3D Inverse() => Value.inverse();
    public CadMatrix3D TranslateWith(CadVector3D vec) => Value.setTranslation(vec);
    public CadVector3D Translation() => Value.translation();

    #endregion
}
#pragma warning restore CA1062
// Copyright © 2018 by Hilti Corporation – all rights reserved
#pragma warning disable CA1062
#nullable enable

using System;
using ODA.Kernel.TD_RootIntegrated;

namespace HCL_ODA_TestPAD.HCL.CAD.Math.API;

public record CadVector3D : CadMathValue<OdGeVector3d>
{
    #region Constructors
    private CadVector3D() { }
    private CadVector3D(Func<OdGeVector3d> factory) : base(factory) { }
    private CadVector3D(OdGeVector3d v) : base(() => new OdGeVector3d(v.x, v.y, v.z)) { }
    protected CadVector3D(CadVector3D other) : base(other)
    {
        Value.Dispose();
        Value = GeWith(other.Value);
    }
    #endregion

    #region Static Factories
    public static OdGeVector3d GeDefault => new();
    public static OdGeVector3d GeWith(double x, double y, double z) => new(x, y, z);
    public static OdGeVector3d GeWith(OdGeVector3d point3d) => new(point3d);
    public static CadVector3D Default => new();
    public static CadVector3D With(CadVector3D v) => new(() => new OdGeVector3d(v.Value));

    public static CadVector3D With(OdGeVector3d v) => new(() => new OdGeVector3d(v.x, v.y, v.z));
    public static CadVector3D With(double x, double y, double z) => new(() => new OdGeVector3d(x, y, z));
    public static OdGeVector3d Origin => GeDefault;
    #endregion

    #region Implicit Operators
    public static implicit operator OdGeVector3d(CadVector3D p) => p.Value;
    public static implicit operator CadVector3D(OdGeVector3d p) => new(p);
    #endregion

    #region Overloaded Operators
    public static CadVector3D operator *(CadVector3D vec, double scale) => vec.Value.Mul(scale);
    public static CadVector3D operator -(CadVector3D vec1, CadVector3D vec2) => vec1.Value.Sub(vec2);
    public static CadVector3D operator +(CadVector3D vec1, CadVector3D vec2) => vec1.Value.Add(vec2);
    public static CadVector3D operator -(CadVector3D vec) => vec.Value.Sub();
    public static CadVector3D operator /(CadVector3D vec, double scale) => vec.Value.Div(scale);
    #endregion

    #region Equality
    public static bool AreEqual(CadVector3D vec1, CadVector3D vec2) => vec1.IsEqualTo(vec2);
    public bool IsEqualTo(CadVector3D vec) => Value.IsEqual(vec);
    public bool IsNotEqualTo(CadVector3D vec) => Value.IsNotEqual(vec);
    public virtual bool Equals(CadVector3D? other) =>
        other is not null &&
        !(IsNotEqual(() => X - other.X) ||
          IsNotEqual(() => Y - other.Y) ||
          IsNotEqual(() => Z - other.Z));

    public override int GetHashCode() => HashCode.Combine(X, Y);
    #endregion

    #region Public Members
    public double X { get => Value.x; init { Value.x = value; } }
    public double Y { get => Value.y; init { Value.y = value; } }
    public double Z { get => Value.z; init { Value.z = value; } }

    #endregion

    #region Public Methods
    public double Length() => Value.length();
    public CadVector3D RotateWith(double angle, CadVector3D axis) => Value.rotateBy(angle, axis);
    public CadVector3D TransformWith(CadMatrix3D matrix) => Value.transformBy(matrix);
    public CadVector3D Mul(double scale) => Value.Mul(scale);
    public CadVector3D PerpendicularVector() => Value.perpVector();
    public CadVector3D CrossProduct(CadVector3D vec) => Value.crossProduct(vec);
    public CadVector3D Normal() => Value.normal();
    public CadVector3D Normalize(OdGeTol tol, out OdGe_ErrorCondition status) => Value.normalize(tol, out status);
    public CadVector3D Normalize() => Value.normalize();
    public double AngleTo(CadVector3D vec, CadVector3D refVector) => Value.angleTo(vec, refVector);
    public double AngleOnPlane(OdGePlanarEnt plane) => Value.angleOnPlane(plane);
    public CadPoint3D AsPoint() => Value.asPoint();
    #endregion

    #region Static Members
    public static CadVector3D XAxis => OdGeVector3d.kXAxis;
    public static CadVector3D YAxis => OdGeVector3d.kYAxis;
    public static CadVector3D ZAxis => OdGeVector3d.kZAxis;

    #endregion
}
#pragma warning restore CA1062
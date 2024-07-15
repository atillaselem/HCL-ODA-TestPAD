// Copyright © 2018 by Hilti Corporation – all rights reserved
#pragma warning disable CA1062
#nullable enable

using System;
using System.Windows;
using ODA.Kernel.TD_RootIntegrated;

namespace HCL_ODA_TestPAD.HCL.CAD.Math.API;

public record CadPoint3D : CadMathValue<OdGePoint3d>
{
    #region Constructors
    private CadPoint3D() { }
    private CadPoint3D(Func<OdGePoint3d> factory) : base(factory) { }
    private CadPoint3D(OdGePoint3d p) : base(() => new OdGePoint3d(p.x, p.y, p.z)) { }
    protected CadPoint3D(CadPoint3D other) : base(other)
    {
        Value.Dispose();
        Value = GeWith(other.Value);
    }
    #endregion

    #region Static Factories
    public static OdGePoint3d GeDefault => new();
    public static OdGePoint3d GeWith(double x, double y, double z) => new(x, y, z);
    public static OdGePoint3d GeWith(OdGePoint3d point3d) => new(point3d);
    public static CadPoint3D Default => new();
    public static CadPoint3D With(CadPoint3D p) => new(() => new OdGePoint3d(p.Value));
    public static CadPoint3D With(OdGePoint3d p) => new(() => new OdGePoint3d(p.x, p.y, p.z));
    public static CadPoint3D With(double x, double y, double z) => new(() => new OdGePoint3d(x, y, z));
    public static CadPoint3D With(Point point) => new(() => new OdGePoint3d(point.X, point.Y, 0));
    public static OdGePoint3d Origin => GeDefault;
    #endregion

    #region Implicit Operators
    public static implicit operator OdGePoint3d(CadPoint3D p) => p.Value;
    public static implicit operator CadPoint3D(OdGePoint3d p) => new(p);
    #endregion

    #region Overloaded Operators
    public static CadPoint3D operator +(CadPoint3D p, CadVector3D v) => p.Value.Add(v);
    public static CadPoint3D operator -(CadPoint3D p, CadVector3D v) => p.Value.Sub(v);
    public static CadVector3D operator -(CadPoint3D pt1, CadPoint3D pt2) => pt1.Value.Sub(pt2);
    public static CadPoint3D operator *(CadPoint3D p, double d) => p.Value.Mul(d);
    #endregion

    #region Equality
    public static bool AreEqual(CadPoint3D pt1, CadPoint3D pt2) => pt1.IsEqualTo(pt2);
    public bool IsEqualTo(CadPoint3D pt) => Value.IsEqual(pt);
    public virtual bool Equals(CadPoint3D? other) =>
        other is not null &&
        !(IsNotEqual(() => X - other.X) ||
        IsNotEqual(() => Y - other.Y) ||
        IsNotEqual(() => Z - other.Z));

    public override int GetHashCode() => HashCode.Combine(X, Y, Z);
    #endregion

    #region Public Members
    public double X { get => Value.x; init { Value.x = value; } }
    public double Y { get => Value.y; init { Value.y = value; } }
    public double Z { get => Value.z; init { Value.z = value; } }
    #endregion

    #region Public Methods
    public double DistanceTo(CadPoint3D point) => Value.distanceTo(point);
    public CadPoint3D TransformWith(CadMatrix3D matrix) => Value.transformBy(matrix);
    public CadPoint3D RotateWith(double angle, CadVector3D axisOfRotation, CadPoint3D basePoint) =>
        Value.rotateBy(angle, axisOfRotation, basePoint);
    public CadPoint3D ScaleWith(double scale) => Value.scaleBy(scale);
    public CadVector3D AsVector() => Value.asVector();
    public CadPoint3D Add(CadVector3D vec) => Value.Add(vec);
    public CadPoint3D SetXyz(double xx, double yy, double zz) => Value.set(xx, yy, zz);
    public CadPoint3D SetToVector(CadVector3D vec) => Value.set(vec.X, vec.Y, vec.Z);
    #endregion
}
#pragma warning restore CA1062
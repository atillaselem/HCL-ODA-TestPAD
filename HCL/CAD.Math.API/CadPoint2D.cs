// Copyright © 2018 by Hilti Corporation – all rights reserved
#pragma warning disable CA1062
#nullable enable

using System;
using ODA.Kernel.TD_RootIntegrated;

namespace HCL_ODA_TestPAD.HCL.CAD.Math.API;

public record CadPoint2D : CadMathValue<OdGePoint2d>
{
    #region Constructors
    private CadPoint2D() { }
    private CadPoint2D(Func<OdGePoint2d> factory) : base(factory) { }
    private CadPoint2D(OdGePoint2d p) : base(() => new OdGePoint2d(p.x, p.y)) { }
    protected CadPoint2D(CadPoint2D other) : base(other)
    {
        Value.Dispose();
        Value = GeWith(other.Value);
    }
    #endregion

    #region Static Factories
    public static OdGePoint2d GeDefault => new();
    public static OdGePoint2d GeWith(double x, double y) => new(x, y);
    public static OdGePoint2d GeWith(OdGePoint2d point2d) => new(point2d);
    public static CadPoint2D Default => new();
    public static CadPoint2D With(CadPoint2D p) => new(() => new OdGePoint2d(p.Value));
    public static CadPoint2D With(OdGePoint2d p) => new(() => new OdGePoint2d(p.x, p.y));
    public static CadPoint2D With(double x, double y) => new(() => new OdGePoint2d(x, y));
    public static OdGePoint2d Origin => GeDefault;
    #endregion

    #region Implicit Operators
    public static implicit operator OdGePoint2d(CadPoint2D p) => p.Value;
    public static implicit operator CadPoint2D(OdGePoint2d p) => new(p);
    #endregion

    #region Overloaded Operators
    public static CadPoint2D operator +(CadPoint2D p, CadVector2D v) => p.Value.Add(v);
    public static CadPoint2D operator -(CadPoint2D p, CadVector2D v) => p.Value.Sub(v);
    public static CadPoint2D operator *(CadPoint2D p, double d) => p.Value.Mul(d);
    #endregion

    #region Equality
    public static bool AreEqual(CadPoint2D pt1, CadPoint2D pt2) => pt1.IsEqualTo(pt2);
    public bool IsEqualTo(CadPoint2D pt) => Value.IsEqual(pt);
    public virtual bool Equals(CadPoint2D? other) =>
        other is not null &&
        !(IsNotEqual(() => X - other.X) ||
          IsNotEqual(() => Y - other.Y));

    public override int GetHashCode() => HashCode.Combine(X, Y);
    #endregion

    #region Public Members
    public double X { get => Value.x; init { Value.x = value; } }
    public double Y { get => Value.y; init { Value.y = value; } }
    #endregion
}
#pragma warning restore CA1062
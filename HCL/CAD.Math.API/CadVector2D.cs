// Copyright © 2018 by Hilti Corporation – all rights reserved
#pragma warning disable CA1062
#nullable enable

using System;
using ODA.Kernel.TD_RootIntegrated;

namespace HCL_ODA_TestPAD.HCL.CAD.Math.API;

public record CadVector2D : CadMathValue<OdGeVector2d>
{
    #region Constructors
    private CadVector2D() { }
    private CadVector2D(Func<OdGeVector2d> factory) : base(factory) { }
    private CadVector2D(OdGeVector2d v) : base(() => new OdGeVector2d(v.x, v.y)) { }
    protected CadVector2D(CadVector2D other) : base(other)
    {
        Value.Dispose();
        Value = GeWith(other.Value);
    }
    #endregion

    #region Static Factories
    public static OdGeVector2d GeDefault => new();
    public static OdGeVector2d GeWith(double x, double y) => new(x, y);
    public static OdGeVector2d GeWith(OdGeVector2d point2d) => new(point2d);
    public static CadVector2D Default => new();
    public static CadVector2D With(CadVector2D v) => new(() => new OdGeVector2d(v.Value));
    public static CadVector2D With(OdGeVector2d v) => new(() => new OdGeVector2d(v.x, v.y));
    public static CadVector2D With(double x, double y) => new(() => new OdGeVector2d(x, y));
    public static OdGeVector2d Origin => GeDefault;
    #endregion

    #region Implicit Operators
    public static implicit operator OdGeVector2d(CadVector2D p) => p.Value;
    public static implicit operator CadVector2D(OdGeVector2d p) => new(p);
    #endregion

    #region Overloaded Operators
    public static CadVector2D operator *(CadVector2D vec, double scale) => vec.Value.Mul(scale);
    public static CadVector2D operator -(CadVector2D vec1, CadVector2D vec2) => vec1.Value.Sub(vec2);
    public static CadVector2D operator +(CadVector2D vec1, CadVector2D vec2) => vec1.Value.Add(vec2);
    public static CadVector2D operator -(CadVector2D vec) => vec.Value.Sub();
    public static CadVector2D operator /(CadVector2D vec, double scale) => vec.Value.Div(scale);
    #endregion

    #region Equality
    public static bool AreEqual(CadVector2D vec1, CadVector2D vec2) => vec1.IsEqualTo(vec2);
    public bool IsEqualTo(CadVector2D vec) => Value.IsEqual(vec);
    public virtual bool Equals(CadVector2D? other) =>
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
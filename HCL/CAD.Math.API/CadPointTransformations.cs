// Copyright © 2018 by Hilti Corporation – all rights reserved

using System.Windows;
using HCL_ODA_TestPAD.HCL.CadUnits;
using ODA.Visualize.TV_Visualize;

namespace HCL_ODA_TestPAD.HCL.CAD.Math.API;

internal static class CadPointTransformations
{
    public static Point ToPoint(this CadPoint3D @this) => new(@this.X, @this.Y);
    public static CadPoint3D ToCadPoint(this Point @this) => CadPoint3D.With(@this.X, @this.Y, 0.0);

    private static CadPoint3D FromWorldToScreen(this CadPoint3D @this, OdTvGsViewId view)
    {
        using var deviceMatrix = view.WorldToDeviceMatrix();
        return @this.TransformWith(deviceMatrix);
    }

    private static CadPoint3D FromScreenToWorld(this CadPoint3D @this, OdTvGsViewId view)
    {
        using var cameraMatrix = view.ScreenToCameraMatrix();
        using var cameraTransformed = @this.TransformWith(cameraMatrix);
        @this.Value.z = 0.0;
        using var worldMatrix = view.EyeToWorldMatrix();
        using var worldTransformed = @this.TransformWith(worldMatrix);
        return @this;
    }
    public static CadPoint3D FromScreenToCamera(this CadPoint3D @this, OdTvGsViewId view)
    {
        using var cameraMatrix = view.ScreenToCameraMatrix();
        using var cameraTransformed = @this.TransformWith(cameraMatrix);
        @this.Value.z = 0.0;
        return @this;
    }
    internal static CadPoint3D FromWorldToScreen(this OdTvGsViewId @this, HcLocation location)
    {
        using var point = CadPoint3D.With(UnitConverter.MetersToMapUnits(location.Easting), UnitConverter.MetersToMapUnits(location.Northing), location.Is2D() ? 0 : UnitConverter.MetersToMapUnits(location.Elevation));
        return point.FromWorldToScreen(@this);
    }

    public static HcLocation FromScreenToWorld(this OdTvGsViewId @this, double x, double y)
    {
        using var cadPointScreen3D = CadPoint3D.With(x, y, 0.0);
        using var cadPointWorld3D = cadPointScreen3D.FromScreenToWorld(@this);

        return UnitConverter.MapUnitsToMeters(new HcLocation(cadPointWorld3D.X, cadPointWorld3D.Y, cadPointWorld3D.Z));
    }
    private static HcLocation FromScreenToWorld(this OdTvGsViewId @this, Point point)
    {
        return @this.FromScreenToWorld(point.X, point.Y);
    }
    private static HcLocation FromScreenToWorld(this OdTvGsViewId @this, Size size)
    {
        return @this.FromScreenToWorld(size.Width, size.Height);
    }

    public static CadPoint3D FromScreenToWorld(this Point @this, OdTvGsViewId view)
    {
        using var cadPointOnScreen = CadPoint3D.With(@this);
        using var cameraMatrix = view.ScreenToCameraMatrix();
        using var cadPointInCameraSpace = cadPointOnScreen.TransformWith(cameraMatrix);
        cadPointInCameraSpace.Value.z = 0.0;
        using var worldMatrix = view.EyeToWorldMatrix();
        var cadPointInWcs = cadPointInCameraSpace.TransformWith(worldMatrix);
        return cadPointInWcs;
    }
}
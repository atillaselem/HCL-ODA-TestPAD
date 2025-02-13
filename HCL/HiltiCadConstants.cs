﻿namespace HCL_ODA_TestPAD.HCL;

using HCL_ODA_TestPAD.Utility;
using System.Drawing;
using System.Windows.Media;
using Color = System.Drawing.Color;

public static class HiltiCadConstants
{
    //Used for file import so only Body context is imported
    public const string BodyContext = "Body";
    public const string MainContext = "Model";
    public static string DefaultModelName = "Model";
}

public static class CadModelConstants
{
    public const string GradientBackground = "Gradient";
    public const string SolidBackground = "Solid";

    public static readonly Color GradientColorTop = Color.FromArgb(108, 178, 224);
    public static readonly Color GradientColorMiddle = Color.FromArgb(163, 208, 236);
    public static readonly Color GradientColorBottom = Color.FromArgb(216, 235, 248);
    public static readonly Color LightColor = Color.White;

    public static readonly Color HighlightColor = Color.FromArgb(171, 1, 21);
    public static readonly Color FaceHighlightColor = Color.FromArgb(255, 175, 0);
    public static readonly Color DashedGreenColor = Color.FromArgb(131, 212, 162);
    public static readonly Color CuttingPlaneFillColor = Color.FromArgb(82, 79, 83);
    public static readonly Color StationingLinesColor = Color.FromArgb(25, 175, 55);
    public static readonly Color StationingUnusedLinesColor = Color.FromArgb(unchecked((int)0xFF979598));

    //public static readonly Color OverrideColor = Color.FromArgb(125, 125, 125);
    public static readonly Color OverrideColor = ResourcesResolver.ResolveResource<SolidColorBrush>("Brushes.CadButton.Border").ToDrawingColor();
    //value from 0 to 255, 255 being fully transparent 
    public static readonly int FaceTransparency = 127;

    //doubles
    public const double ScaleCoordinate1X = 0.0;
    public const double ScaleCoordinate1Y = 0.0;
    public const double ScaleCoordinate2X = 10.0;
    public const double ScaleCoordinate2Y = 0.0;
    public const double ScaleDistanceLimit = 5.0;
    public const double MinScaleCoef = 1.0e-10;

    /// <summary>
    /// To avoid visible artifacts visible cutting plane should be offset
    /// </summary>
    public const double VisibleCuttingPlaneOffset = 1e-1;

    public const double CadInvalidSize = 1e15;
    public const double PointComparisonTolerance = 1e-3;

    public const string DefaultAppName = "App";
    public const string PointIdentifier = "Point";

    /// <summary>
    /// The max zoom scale for the Device Width is 0.1m
    /// </summary>
    public const double MaxWidthScaleInMeters = 0.1;

    /// <summary>
    /// The time span in milliseconds When to regenerate the view
    /// </summary>
    public const int TimeSpanInMilliseconds = 1000;

    /// <summary>
    /// The threshold distance the mouse-cursor must move before drag-selection begins.
    /// </summary>
    public const double DragThreshold = 5;

    public const double ZoomScale = 0.0;
    public const int PanEntitySelectionTime = 200;
    public const float ZoomThresholdDistance = 1f;

    #region Custom point Representation
    public const double TriangleBase = 5;
    public const double FontSize = 1;
    public const double PointGripSizeLimit = 0.000002;
    #endregion

    #region Point Block Names
    /// <summary>
    /// Assign point name with respective point type
    /// while saving points as blocks
    /// </summary>
    public const string HcLayoutPtBlockString = "HCLayoutPointBlock";
    public const string HcControlPtBlockString = "HCControlPointBlock";
    public const string HcMeasurePtBlockString = "HCMeasurePointBlock";
    public const string HcStationPtBlockString = "HCStationPointBlock";
    #endregion

    #region Unit Conversion Values
    public const double DefaultMeters = 1.0;
    public const double Feet2Meters = 0.30480;
    public const double UsFeet2Meter = 0.304800609601219;
    public const double Inches2Meters = 0.0254;
    public const double Millimeters2Meters = 0.001;
    public const double Centimeters2Meters = 0.01;
    public const double Meters2Inches = 39.37007874015748;
    public const double Meters2Feet = DefaultMeters / Feet2Meters;
    public const double Meter2UsFeet = DefaultMeters / UsFeet2Meter;
    public const double Meters2Millimeters = 1000;
    public const double Meters2Centimeters = 100;
    public const double UsFeetCoefValue = 0.30480060960121919;
    #endregion

    /// <summary>
    /// Represents minimal size of cad screen after zoom extents
    /// </summary>
    public const double MinCadSize = 0.1;

    /// <summary>
    /// Represents epsilon value for calculation proper zoom extents
    /// </summary>
    public const double ZoomExtentsEpsilon = 0.01;

    /// <summary>
    /// meter unit point size and text height while dwg export
    /// </summary>
    public const double PointSizeDwgExportMeter = 0.1;

    /// <summary>
    /// Tag for northing image file name
    /// </summary>
    public const string NorthingIconName = "northing.png";

    /// <summary>
    /// Tag for PLT/POS tool image file name
    /// </summary>
    public const string PltIconName = "station-plt.png";
    public const string PosIconName = "station-pos.png";
    public const string HPLReflector_CatEye360 = "CatEye360.png";
    public const string HPLReflector_CatEyePrism = "CatEyePrism.png";
    public const string HPLReflector_GlassPrism360 = "GlassPrism360.png";
    public const string HPLReflector_LaserPrism = "LaserPrism.png";
    public const string HPLReflector_MiniGlassPrism360 = "MiniGlassPrism360.png";
    public const string HPLReflector_OverAllPrism = "OverAllPrism.png";
    public const string HPLReflector_POA101 = "POA101.png";
    public const string HPLReflector_POA102 = "POA102.png";
    public const string HPLReflector_POA103 = "POA103.png";
    public const string HPLReflector_ReflectivePlate = "ReflectivePlate.png";
    public const string HPLReflector_ReflectiveSticker = "ReflectiveSticker.png";
    public const string HPLReflector_SlidingPrism = "SlidingPrism.png";
    public const string HPLReflector_WallPrism = "WallPrism.png";
    public const string DashedStyle = "DashedStyle";
    public const string ShortDash = "ShortDash";
    public const string MidiumDashedStyle = "MidiumDashedStyle";
    /// <summary>
    /// In Visualize SDK "0" layer is not supported, "ZeroLayerName" is used instead so we need to perform manual layer name conversion
    /// </summary>
    public const string ZeroLayerName = "ZeroLayerName";
    public const string NumericalZeroLayerName = "0";

    public const double ExtentsRange = 1E+20;
}
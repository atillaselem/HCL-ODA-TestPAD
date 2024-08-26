using System.ComponentModel;
using HCL_ODA_TestPAD.HCL.CadUnits;
using HCL_ODA_TestPAD.HCL.Visualize;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using ODA.Visualize.TV_Visualize;
using System.ComponentModel.DataAnnotations;

namespace HCL_ODA_TestPAD.Settings;

public enum AppLayout
{
    Default,
    Dockable
}
public enum RenderDevice
{
    OpenGlBitmap,
    OpenGlGpu
}

public enum SettingsMode
{
    Hcl,
    Custom
}

public enum HPLReflector
{
    CatEye360,
    CatEyePrism,
    GlassPrism360,
    LaserPrism,
    MiniGlassPrism360,
    OverAllPrism,
    POA101,
    POA102,
    POA103,
    ReflectivePlate,
    ReflectiveSticker,
    SlidingPrism,
    WallPrism
}

[CategoryOrder("OpenGLES2 Device", 1)]
[CategoryOrder("Regeneration", 2)]
[CategoryOrder("Performance", 3)]
[CategoryOrder("DWG & DXF Import Parameters", 4)]
[CategoryOrder("IFC Import Parameters", 5)] 
[CategoryOrder("CAD Units", 6)] 
[CategoryOrder("HCL CAD Domain", 7)]
[CategoryOrder("HCL Point Optimization", 8)]
[CategoryOrder("CAD Custom Models", 9)] 
[CategoryOrder("User Interface", 10)] 
[CategoryOrder("On Start", 11)]
[CategoryOrder("About App", 12)] 
[CategoryOrder("Preferences", 13)] 

public record AppSettings
{
    #region OpenGLES2 Device
    [Category("OpenGLES2 Device")]
    [DisplayName("Frozen Layers Visible")]
    [Description("Set Frozen Layers Visible")]
    public bool SetFrozenLayersVisible { get; init; }

    [Category("OpenGLES2 Device")]
    [DisplayName("Scene Graph")]
    [Description("Enable SceneGraph")]
    public bool UseSceneGraph { get; init; }

    [Category("OpenGLES2 Device")]
    [DisplayName("Render Device")]
    [Description("Render device type to render OpenGLES2")]
    public RenderDevice RenderDevice { get; init; }

    [Category("OpenGLES2 Device")]
    [DisplayName("Force Partial Update")]
    [Description("Enable ForcePartialUpdate")]
    public bool UseForcePartialUpdate { get; init; }

    [Category("OpenGLES2 Device")]
    [DisplayName("Blocks Cache")]
    [Description("Enable BlocksCache")]
    public bool UseBlocksCache { get; init; }

    [Category("OpenGLES2 Device")]
    [DisplayName("Force Offscreen SceneGraph")]
    [Description("Enable Force Offscreen SceneGraph")]
    public bool UseForceOffscreenSceneGraph { get; init; }

    [Category("OpenGLES2 Device")]
    [DisplayName("SceneGraph Purge Grouped Renders")]
    [Description("Purge Grouped Renders")]
    public bool UseSceneGraphPurgeGroupedRenders { get; init; }

    [Category("OpenGLES2 Device")]
    [DisplayName("Use Visual Styles")]
    [Description("If True Use Visual Styles, else use only Render Modes")]
    public bool UseVisualStyles { get; init; }

    [Category("OpenGLES2 Device")]
    [DisplayName("Forbid Image Highlight")]
    [Description("Set Forbid Image Highlight")]
    public bool SetForbidImageHighlight { get; init; }
    #endregion

    #region Regeneration
    [Category("Regeneration")]
    [DisplayName("Auto Regeneration")]
    [Description("Enables Auto Regenaration >= Regeneration Threshold")]
    public bool AutoRegeneration { get; init; }

    [Category("Regeneration")]
    [DisplayName("Regeneration Threshold")]
    [Description("Set Regeneration Threshold")]
    public double RegenThreshold { get; init; }

    [Category("Regeneration")]
    [DisplayName("RegenMode")]
    [Description("Set Regeneration Mode")]
    public OdTvGsDevice_RegenMode RegenMode { get; init; }
    #endregion

    #region Performance
    [Category("Performance")]
    [DisplayName("Interactivity")]
    [Description("Enable Interacivity")]
    public bool Interactivity { get; init; }

    [Category("Performance")]
    [DisplayName("Use CadProfiler")]
    [Description("Enable CadProfiler used during Debugging or Logging")]
    public bool UseCadProfiler { get; init; }

    [Category("Performance")]
    [DisplayName("Interactive FPS")]
    [Description("Set minimum FPS during Interactivity")]
    public double InteractiveFps { get; init; }
    #endregion

    #region DWG & DXF Import Parameters
    [Category("DWG & DXF Import Parameters")]
    [DisplayName("Object Naming")]
    [Description("Set Object Naming")]
    public bool DwgSetObjectNaming { get; init; }
    [Category("DWG & DXF Import Parameters")]
    [DisplayName("Store Source Objects")]
    [Description("Set StoreSourceObjects")]
    public bool DwgSetStoreSourceObjects { get; init; }
    [Category("DWG & DXF Import Parameters")]
    [DisplayName("Import Frozen Layers")]
    [Description("Set ImportFrozenLayers")]
    public bool DwgSetImportFrozenLayers { get; init; }
    [Category("DWG & DXF Import Parameters")]
    [DisplayName("Clear Empty Objects")]
    [Description("Set ClearEmptyObjects")]
    public bool DwgSetClearEmptyObjects { get; init; }
    [Category("DWG & DXF Import Parameters")]
    [DisplayName("Need CDA Tree")]
    [Description("Set Object Naming")]
    public bool DwgSetNeedCdaTree { get; init; }
    [Category("DWG & DXF Import Parameters")]
    [DisplayName("Need Collect Properties In CDA")]
    [Description("Set NeedCollectPropertiesInCDA")]
    public bool DwgSetNeedCollectPropertiesInCda { get; init; }
    #endregion

    #region IFC Import Parameters 
    [Category("IFC Import Parameters")]
    [DisplayName("Use Scene Graph")]
    [Description("Use Scene Graph for IFC Files")]
    public bool IfcUseSceneGraph { get; init; }
    [Category("IFC Import Parameters")]
    [DisplayName("Need CDA Tree")]
    [Description("Set Object Naming")]
    public bool IfcSetNeedCdaTree { get; init; }
    [Category("IFC Import Parameters")]
    [DisplayName("Need Collect Properties In CDA")]
    [Description("Set NeedCollectPropertiesInCDA")]
    public bool IfcSetNeedCollectPropertiesInCda { get; init; }
    #endregion
    
    #region CAD Units
    [Category("CAD Units")]
    [DisplayName("Cad File Unit")]
    [Description("Survey Unit set by reading from Cad File")]
    [ReadOnly(true)]
    public SurveyUnits CadFileUnit { get; init; }
    [Category("CAD Units")]
    [DisplayName("Import Unit Change")]
    [Description("Enables CAD Survey Unit change during CAD file loading/importing")]
    public bool EnableImportUnitChange { get; init; }
    [Category("CAD Units")]
    [DisplayName("Cad Import Unit")]
    [Description("Selected Survey Unit will be set for the imported CAD File Unit. Original CAD Unit of the file will not change.")]
    public SurveyUnits CadImportUnit { get; init; }
    #endregion

    #region HCL CAD Domain
    [Category("HCL CAD Domain")]
    [DisplayName("HPL Reflector")]
    [Description("Active Prism Type used as Reflector")]
    public HPLReflector PrismType { get; init; }
    [Category("HCL CAD Domain")]
    [DisplayName("Number of Points")]
    [Description("Set the number of points created")]
    [Range(1, 10000, ErrorMessage = "Value should be grater than 0 and less than 10000")]
    public uint NumberOfPoints { get; init; } = 1;
    [Category("HCL CAD Domain")]
    [DisplayName("Random Point Color")]
    [Description("Enables random colors for the points to be created")]
    public bool IsRandomColor { get; init; }
    [Category("HCL CAD Domain")]
    [DisplayName("Point Color")]
    [Description("Color of the points to be created if not random")]
    public PointColor PointColor { get; init; }
    #endregion

    #region HCL Point Optimization
    [Category("HCL Point Optimization")]
    [DisplayName("Use Point Optimization")]
    [Description("Enables point rendering optimized")]
    public bool EnablePointOptimization { get; init; }
    [Category("HCL Point Optimization")]
    [DisplayName("Dynamic Optimization")]
    [Description("Use best optimization according number of points")]
    public bool UseDynamicOptimization { get; init; }
    [Category("HCL Point Optimization")]
    [DisplayName("Render Every X Cycle")]
    [Description("Render points in every X Cycle")]
    [Range(1, 20, ErrorMessage = "Value should be grater than 0 and less than 10")]
    public uint RenderEveryXCycle { get; init; } = 1;
    [Category("HCL Point Optimization")]
    [DisplayName("Hide Point Text Transformation")]
    [Description("Hide Point Text during scaling and rotation")]
    public bool HidePointTextTransformation { get; init; }
    #endregion

    #region CAD Custom Models
    [Category("CAD Custom Models")]
    [DisplayName("FPS")]
    [Description("Show FPS model")]
    public bool ShowFps { get; init; }
    [Category("CAD Custom Models")]
    [DisplayName("WCS")]
    [Description("Show World Coordinate System model")]
    public bool ShowWcs { get; init; }
    [Category("CAD Custom Models")]
    [DisplayName("View Cube")]
    [Description("Show View Cube model")]
    public bool ShowCube { get; init; }
    #endregion
    
    #region User Interface
    [Category("User Interface")]
    [DisplayName("UI Layout")]
    [Description("Select User Interface Layout")]
    [ReadOnly(true)]
    public AppLayout AppLayout { get; init; }

    [Category("User Interface")]
    [DisplayName("Auto Save Layout")]
    [Description("Auto saves Dockable Pane Layout on Exit")]
    [ReadOnly(true)]
    public bool SaveDockLayout { get; init; } = true;
    #endregion

    #region On Start
    [Category("On Start")]
    [DisplayName("Splash Screen")]
    [Description("Shows Splash screen on Start")]
    public bool ShowSplashScreen { get; init; }
    #endregion Start
    
    #region About App
    [Category("About App")]
    [DisplayName("About App")]
    [Description("Shows About Info animated")]
    [ReadOnly(true)]
    public bool ShowAboutAnimation { get; init; }
    #endregion About App

    #region Preferences
    [Category("Preferences")]
    [DisplayName("Auto Save Settings")]
    [Description("Auto saves setting changes on Exit")]
    [ReadOnly(true)]
    public bool SaveSettings { get; init; } = true;

    [Category("Preferences")]
    [DisplayName("Settings Mode")]
    [Description("Choose mode as HCL to reset custom settings")]
    public SettingsMode SettingsMode { get; init; }
    #endregion

}
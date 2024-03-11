using System.ComponentModel;
using HCL_ODA_TestPAD.HCL.CadUnits;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using ODA.Visualize.TV_Visualize;

namespace HCL_ODA_TestPAD.Settings;

public enum AppLayout
{
    Default,
    Dockable
}
public enum RenderDevice
{
    OpenGL_Bitmap,
    OpenGL_GPU
}

public enum SettingsMode
{
    HCL,
    Custom
}

[CategoryOrder("OpenGLES2 Device", 1)]
[CategoryOrder("Regeneration", 2)]
[CategoryOrder("Performance", 3)]
[CategoryOrder("DWG & DXF Import Parameters", 4)]
[CategoryOrder("IFC Import Parameters", 5)] 
[CategoryOrder("CAD Units", 6)] 
[CategoryOrder("CAD Custom Models", 7)] 
[CategoryOrder("User Interface", 8)] 
[CategoryOrder("On Start", 9)]
[CategoryOrder("About App", 10)] 
[CategoryOrder("Preferences", 11)] 

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
    [DisplayName("Interactive FPS")]
    [Description("Set minimum FPS during Interactivity")]
    public double InteractiveFPS { get; init; }
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
    public bool DwgSetNeedCDATree { get; init; }
    [Category("DWG & DXF Import Parameters")]
    [DisplayName("Need Collect Properties In CDA")]
    [Description("Set NeedCollectPropertiesInCDA")]
    public bool DwgSetNeedCollectPropertiesInCDA { get; init; }
    #endregion

    #region IFC Import Parameters 
    [Category("IFC Import Parameters")]
    [DisplayName("Use Scene Graph")]
    [Description("Use Scene Graph for IFC Files")]
    public bool IfcUseSceneGraph { get; init; }
    [Category("IFC Import Parameters")]
    [DisplayName("Need CDA Tree")]
    [Description("Set Object Naming")]
    public bool IfcSetNeedCDATree { get; init; }
    [Category("IFC Import Parameters")]
    [DisplayName("Need Collect Properties In CDA")]
    [Description("Set NeedCollectPropertiesInCDA")]
    public bool IfcSetNeedCollectPropertiesInCDA { get; init; }
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
    
    #region CAD Custom Models
    [Category("CAD Custom Models")]
    [DisplayName("FPS")]
    [Description("Show FPS model")]
    public bool ShowFPS { get; init; }
    [Category("CAD Custom Models")]
    [DisplayName("WCS")]
    [Description("Show World Coordinate System model")]
    public bool ShowWCS { get; init; }
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
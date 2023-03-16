using System.ComponentModel;
using HCL_ODA_TestPAD.HCL.CadUnits;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using static Teigha.Visualize.OdTvGsDevice;

namespace HCL_ODA_TestPAD.Settings;

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

public record AppSettings : IAppSettings
{
    #region Start
    [Category("On Start")]
    [DisplayName("Splash Screen")]
    [Description("Shows Splash screen on Start")]
    public bool ShowSplashScreen { get; set; }
    #endregion Start

    #region About App
    [Category("About App")]
    [DisplayName("About App")]
    [Description("Shows About Info animated")]
    [ReadOnly(true)]
    public bool ShowAboutAnimation { get; set; }
    #endregion About App

    #region CAD Units
    [Category("CAD Units")]
    [DisplayName("Cad File Unit")]
    [Description("Survey Units used for Cad File")]
    public SurveyUnits CadFileUnit { get; set; }
    #endregion

    #region UI Layout
    [Category("User Interface")]
    [DisplayName("UI Layout")]
    [Description("Select User Interface Layout")]
    [ReadOnly(true)]
    public AppLayout AppLayout { get; set; }

    [Category("User Interface")]
    [DisplayName("Auto Save Layout")]
    [Description("Auto saves Dockable Pane Layout on Exit")]
    [ReadOnly(true)]
    public bool SaveDockLayout { get; set; } = true;
    #endregion

    #region OpenGLES2 Device
    [Category("OpenGLES2 Device")]
    [DisplayName("Frozen Layers Visible")]
    [Description("Set Frozen Layers Visible")]
    public bool SetFrozenLayersVisible { get; set; }

    [Category("OpenGLES2 Device")]
    [DisplayName("Scene Graph")]
    [Description("Enable SceneGraph")]
    public bool UseSceneGraph { get; set; }

    [Category("OpenGLES2 Device")]
    [DisplayName("Render Device")]
    [Description("Render device type to render OpenGLES2")]
    public RenderDevice RenderDevice { get; set; }

    [Category("OpenGLES2 Device")]
    [DisplayName("Force Partial Update")]
    [Description("Enable ForcePartialUpdate")]
    public bool UseForcePartialUpdate { get; set; }

    [Category("OpenGLES2 Device")]
    [DisplayName("Forbid Image Highlight")]
    [Description("Set Forbid Image Highlight")]
    public bool UseBlocksCache { get; set; }

    [Category("OpenGLES2 Device")]
    [DisplayName("Blocks Cache")]
    [Description("Enable BlocksCache")]
    public bool SetForbidImageHighlight { get; set; }
    #endregion

    #region Regeneration
    [Category("Regeneration")]
    [DisplayName("Auto Regeneration")]
    [Description("Enables Auto Regenaration >= Regeneration Threshold")]
    public bool AutoRegeneration { get; set; }

    [Category("Regeneration")]
    [DisplayName("Regeneration Threshold")]
    [Description("Set Regeneration Threshold")]
    public double RegenThreshold { get; set; }

    [Category("Regeneration")]
    [DisplayName("RegenMode")]
    [Description("Set Regeneration Mode")]
    public RegenMode RegenMode { get; set; }
    #endregion

    #region Performance
    [Category("Performance")]
    [DisplayName("Interactivity")]
    [Description("Enable Interacivity")]
    public bool Interactivity { get; set; }

    [Category("Performance")]
    [DisplayName("Interactive FPS")]
    [Description("Set minimum FPS during Interactivity")]
    public double InteractiveFPS { get; set; }
    #endregion

    #region DWG & DXF Import Parameters
    [Category("DWG & DXF Import Parameters")]
    [DisplayName("Object Naming")]
    [Description("Set Object Naming")]
    public bool DwgSetObjectNaming { get; set; }
    [Category("DWG & DXF Import Parameters")]
    [DisplayName("Store Source Objects")]
    [Description("Set StoreSourceObjects")]
    public bool DwgSetStoreSourceObjects { get; set; }
    [Category("DWG & DXF Import Parameters")]
    [DisplayName("Import Frozen Layers")]
    [Description("Set ImportFrozenLayers")]
    public bool DwgSetImportFrozenLayers { get; set; }
    [Category("DWG & DXF Import Parameters")]
    [DisplayName("Clear Empty Objects")]
    [Description("Set ClearEmptyObjects")]
    public bool DwgSetClearEmptyObjects { get; set; }
    [Category("DWG & DXF Import Parameters")]
    [DisplayName("Need CDA Tree")]
    [Description("Set Object Naming")]
    public bool DwgSetNeedCDATree { get; set; }
    [Category("DWG & DXF Import Parameters")]
    [DisplayName("Need Collect Properties In CDA")]
    [Description("Set NeedCollectPropertiesInCDA")]
    public bool DwgSetNeedCollectPropertiesInCDA { get; set; }
    #endregion

    #region IFC Import Parameters 
    [Category("IFC Import Parameters")]
    [DisplayName("Use Scene Graph")]
    [Description("Use Scene Graph for IFC Files")]
    public bool IfcUseSceneGraph { get; set; }
    [Category("IFC Import Parameters")]
    [DisplayName("Need CDA Tree")]
    [Description("Set Object Naming")]
    public bool IfcSetNeedCDATree { get; set; }
    [Category("IFC Import Parameters")]
    [DisplayName("Need Collect Properties In CDA")]
    [Description("Set NeedCollectPropertiesInCDA")]
    public bool IfcSetNeedCollectPropertiesInCDA { get; set; }
    #endregion

    #region CAD Custom Models
    [Category("CAD Custom Models")]
    [DisplayName("FPS")]
    [Description("Show FPS model")]
    public bool ShowFPS { get; set; }
    [Category("CAD Custom Models")]
    [DisplayName("WCS")]
    [Description("Show World Coordinate System model")]
    public bool ShowWCS { get; set; }
    [Category("CAD Custom Models")]
    [DisplayName("View Cube")]
    [Description("Show View Cube model")]
    public bool ShowCube { get; set; }
    #endregion

    #region Preferences
    [Category("Preferences")]
    [DisplayName("Auto Save Settings")]
    [Description("Auto saves setting changes on Exit")]
    [ReadOnly(true)]
    public bool SaveSettings { get; set; } = true;

    [Category("Preferences")]
    [DisplayName("Settings Mode")]
    [Description("Choose mode as HCL to reset custom settings")]
    public SettingsMode SettingsMode { get; set; }
    #endregion
}
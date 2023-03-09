using System.ComponentModel;
using HCL_ODA_TestPAD.HCL.CadUnits;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace HCL_ODA_TestPAD.Settings;

[CategoryOrder("OpenGLES2 Device", 1)]
[CategoryOrder("DWG & DXF Import Parameters", 2)]
[CategoryOrder("IFC Import Parameters", 3)] 
[CategoryOrder("CAD Units", 4)] 
[CategoryOrder("CAD Custom Models", 5)] 
[CategoryOrder("User Interface", 6)] 
[CategoryOrder("Start App", 7)]
[CategoryOrder("Exit App", 8)] 
[CategoryOrder("About App", 9)] 
//[CategoryOrder("Settings Type", 10)] 

public record AppSettings : IAppSettings
{
    #region Start App
    /// <summary>
    /// Gets or sets a value indicating whether [show splash screen].
    /// </summary>
    /// <value>
    ///   <c>true</c> if [show splash screen]; otherwise, <c>false</c>.
    /// </value>
    [Category("Start App")]
    [DisplayName("Show Splash Screen")]
    [Description("Shows Splash screen on application startup.")]
    public bool ShowSplashScreen { get; set; }

    #endregion Start App

    #region About App
    /// <summary>
    /// Gets or sets a value indicating whether [show about].
    /// </summary>
    /// <value>
    ///   <c>true</c> if [show splash screen]; otherwise, <c>false</c>.
    /// </value>
    [Category("About App")]
    [DisplayName("Show About Animated")]
    [Description("Shows About animated")]
    [ReadOnly(true)]
    public bool ShowAboutAnimation { get; set; }
    #endregion About App

    #region Exit App

    /// <summary>
    /// Gets or sets a value indicating whether [save settings].
    /// </summary>
    /// <value>
    ///   <c>true</c> if [save settings]; otherwise, <c>false</c>.
    /// </value>
    [Category("Exit App")]
    [DisplayName("Save Settings")]
    [Description("Saves Settings with current state during Application Closing")]
    [ReadOnly(true)]
    public bool SaveSettings { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether [save dock layout].
    /// </summary>
    /// <value>
    ///   <c>true</c> if [save dock layout]; otherwise, <c>false</c>.
    /// </value>
    [Category("Exit App")]
    [DisplayName("Save Dock Layout")]
    [Description("Saves Dockable Pane Layout during Application Closing")]
    [ReadOnly(true)]
    public bool SaveDockLayout { get; set; } = true;

    #endregion Exit App

    #region CAD Units
    /// <summary>
    /// Gets or sets a value indicating whether [show splash screen].
    /// </summary>
    /// <value>
    ///   <c>true</c> if [show splash screen]; otherwise, <c>false</c>.
    /// </value>
    [Category("CAD Units")]
    [DisplayName("Cad File Unit")]
    [Description("Survey Units used for Cad File")]
    public SurveyUnits CadFileUnit { get; set; }
    #endregion

    #region UI Layout
    /// <summary>
    /// Gets or sets a value indicating whether [show splash screen].
    /// </summary>
    /// <value>
    ///   <c>true</c> if [show splash screen]; otherwise, <c>false</c>.
    /// </value>
    [Category("User Interface")]
    [DisplayName("UI Layout")]
    [Description("User Interface Layout")]
    //[ReadOnly(true)]
    public AppLayout AppLayout { get; set; }

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
    public bool SetForbidImageHighlight { get; set; }
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

    #region Settings Type
    //#Settings 
    [Category("Settings Mode")]
    [DisplayName("Settings Mode")]
    [Description("Choose HCL to reset custom settings")]
    public SettingsMode SettingsMode { get; set; }
    #endregion
}
using System.ComponentModel;
using HCL_ODA_TestPAD.EventBroker;
using HCL_ODA_TestPAD.HCL.CadUnits;
using HCL_ODA_TestPAD.Mvvm.Events;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace HCL_ODA_TestPAD.Settings;

[CategoryOrder("OpenGLES2 Device", 1)]
[CategoryOrder("DWG Import Parameters", 2)]
[CategoryOrder("IFC Import Parameters", 3)] 
[CategoryOrder("CAD Units", 4)] 
[CategoryOrder("User Interface", 5)] 
[CategoryOrder("Start App", 6)]
[CategoryOrder("Exit App", 7)] 
[CategoryOrder("About App", 8)] 
public class AppSettings : IAppSettings
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
    [ReadOnly(true)]
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
    [ReadOnly(true)]
    public RenderDevice RenderDevice { get; set; } = RenderDevice.Bitmap;

    [Category("OpenGLES2 Device")]
    [DisplayName("Force Partial Update")]
    [Description("Enable ForcePartialUpdate")]
    public bool UseForcePartialUpdate { get; set; }
    #endregion

    #region DWG Import Parameters
    [Category("DWG Import Parameters")]
    [DisplayName("Object Naming")]
    [Description("Set Object Naming")]
    public bool DwgSetObjectNaming { get; set; } = true;
    [Category("DWG Import Parameters")]
    [DisplayName("Store Source Objects")]
    [Description("Set StoreSourceObjects")]
    public bool DwgSetStoreSourceObjects { get; set; }
    [Category("DWG Import Parameters")]
    [DisplayName("Import Frozen Layers")]
    [Description("Set ImportFrozenLayers")]
    public bool DwgSetImportFrozenLayers { get; set; } = true;
    [Category("DWG Import Parameters")]
    [DisplayName("Clear Empty Objects")]
    [Description("Set ClearEmptyObjects")]
    public bool DwgSetClearEmptyObjects { get; set; } = true;
    [Category("DWG Import Parameters")]
    [DisplayName("Need CDA Tree")]
    [Description("Set Object Naming")]
    public bool DwgSetNeedCDATree { get; set; }
    [Category("DWG Import Parameters")]
    [DisplayName("Need Collect Properties In CDA")]
    [Description("Set NeedCollectPropertiesInCDA")]
    public bool DwgSetNeedCollectPropertiesInCDA { get; set; }
    #endregion

    #region IFC Import Parameters 
    [Category("IFC Import Parameters")]
    [DisplayName("Need CDA Tree")]
    [Description("Set Object Naming")]
    public bool IfcSetNeedCDATree { get; set; }
    [Category("IFC Import Parameters")]
    [DisplayName("Need Collect Properties In CDA")]
    [Description("Set NeedCollectPropertiesInCDA")]
    public bool IfcSetNeedCollectPropertiesInCDA { get; set; }
    #endregion

    public void UpdateUserInterface()
    {
        FxBroker<AET>.Instance.Publish(this, AET.EVENT_UPDATE_SETTINGS_UI, null);
    }
}
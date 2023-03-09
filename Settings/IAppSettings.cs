using HCL_ODA_TestPAD.HCL.CadUnits;

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
public interface IAppSettings
{
    //#region Start App
    bool ShowSplashScreen { get; set; }
    //#region About App
    bool ShowAboutAnimation { get; set; }
    //#region Exit App
    bool SaveSettings { get; set; }
    bool SaveDockLayout { get; set; }
    //#region CAD Units
    SurveyUnits CadFileUnit { get; set; }
    //#region UI
    AppLayout AppLayout { get; set; }
    //#Graphics Device
    RenderDevice RenderDevice { get; set; }
    bool SetFrozenLayersVisible { get; set; }
    bool UseSceneGraph { get; set; }
    bool UseForcePartialUpdate { get; set; }
    bool SetForbidImageHighlight { get; set; }
    //#DWG & DXF Import Parameters
    bool DwgSetObjectNaming { get; set; }
    bool DwgSetStoreSourceObjects { get; set; }
    bool DwgSetImportFrozenLayers { get; set; }
    bool DwgSetClearEmptyObjects { get; set; }
    bool DwgSetNeedCDATree { get; set; }
    bool DwgSetNeedCollectPropertiesInCDA { get; set; }
    //#IFC Import Parameters
    bool IfcUseSceneGraph { get; set; }
    bool IfcSetNeedCDATree { get; set; }
    bool IfcSetNeedCollectPropertiesInCDA { get; set; }
    //#Show Custom Models
    bool ShowFPS { get; set; }
    bool ShowWCS { get; set; }
    bool ShowCube { get; set; }
    //#Settings 
    SettingsMode SettingsMode { get; set; }
}
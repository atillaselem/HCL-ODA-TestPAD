using System.Configuration;
using System.IO;
using HCL_ODA_TestPAD.Mvvm.Events;
using HCL_ODA_TestPAD.Utility;
using Newtonsoft.Json;

namespace HCL_ODA_TestPAD.Settings;

public class SettingsProvider : ISettingsProvider
{
    public const string APP_SETTINGS_FOLDER = @"common\config";
    public const string APP_SETTINGS_FILE = @"common\config\HCL_ODA_TestPAD_AppSettings.json";
    public const string HCL_APP_SETTINGS_FILE = @"common\config\HCL_ODA_TestPAD_AppSettings_HCL.json";
    public const string APP_SETTINGS_BACKUP_FILE = @"common\config\_backUp\HCL_ODA_TestPAD_AppSettings.json";
    public const string APP_DOCKPANELLAYOUT_FILE = @"common\config\HCL_ODA_TestPAD_AppLayout.xml";
    public const string APP_DOCK_PANEL_LAYOUT_BACKUP_FILE = @"common\config\_backUp\HCL_ODA_TestPAD_AppLayout.xml";

    public SettingsProvider()
    {
        _appSettingsHCL = SettingsSerializer<AppSettings>.Load(PathResolver.GetTargetPathUsingRelativePath(HCL_APP_SETTINGS_FILE));
        LoadSettings();
    }
    public AppSettings AppSettings { get; set; }

    private readonly AppSettings _appSettingsHCL;

    /// <summary>
    ///
    /// </summary>
    private void LoadSettings()
    {
        CheckAppSettingsFolder();
        AppSettings = SettingsSerializer<AppSettings>.Load(PathResolver.GetTargetPathUsingRelativePath(APP_SETTINGS_FILE));
        if (AppSettings == null)
        {
            AppSettings = _appSettingsHCL with { SettingsMode = SettingsMode.HCL, CadFileUnit = _appSettingsHCL.CadFileUnit };
        }
    }

    private void CheckAppSettingsFolder()
    {
        var appSettingsFolder = PathResolver.GetTargetPathUsingRelativePath(APP_SETTINGS_FOLDER);
        if (!Directory.Exists(appSettingsFolder))
        {
            Directory.CreateDirectory(appSettingsFolder);
            //var backUpAppDockPanelLayoutFile = RecursivePathFinder.SearchFileFolderPath(APP_DOCK_PANEL_LAYOUT_BACKUP_FILE);
            //var appDockLayoutFile = PathResolver.GetTargetPathUsingRelativePath(APP_DOCKPANELLAYOUT_FILE);
            //File.Copy(backUpAppDockPanelLayoutFile, appDockLayoutFile);
        }
    }

    public void SaveSettings(AppSettings appSettings)
    {
        SettingsSerializer<AppSettings>.Save(appSettings, PathResolver.GetTargetPathUsingRelativePath(APP_SETTINGS_FILE));
    }

    public AppSettings CompareSettings(AppSettings appSettings, string itemValue)
    {
        // Reset settings by selecting Mode : HCL
        if (itemValue == SettingsMode.HCL.ToString())
        {
            return _appSettingsHCL with { CadFileUnit = appSettings.CadFileUnit };
        }

        // If all settings set back to equal HCL return by only changing Mode : HCL
        var tempSettings = appSettings with { SettingsMode = SettingsMode.HCL };
        if (tempSettings == _appSettingsHCL)
        {
            return _appSettingsHCL with {}; // every property was copied
        }
        
        //If any change of settings change Mode : Custom
        return appSettings with { SettingsMode = SettingsMode.Custom };
    }
}
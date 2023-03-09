using System.IO;
using HCL_ODA_TestPAD.Extensions;
using HCL_ODA_TestPAD.Mvvm.Events;
using HCL_ODA_TestPAD.Utility;
using Newtonsoft.Json;

namespace HCL_ODA_TestPAD.Settings;

public class SettingsLoader
{
    public const string APP_SETTINGS_FOLDER = @"common\config";
    public const string APP_SETTINGS_FILE = @"common\config\HCL_ODA_TestPAD_AppSettings.json";
    public const string HCL_APP_SETTINGS_FILE = @"common\config\HCL_ODA_TestPAD_AppSettings_HCL.json";
    public const string APP_SETTINGS_BACKUP_FILE = @"common\config\_backUp\HCL_ODA_TestPAD_AppSettings.json";
    public const string APP_DOCKPANELLAYOUT_FILE = @"common\config\HCL_ODA_TestPAD_AppLayout.xml";
    public const string APP_DOCK_PANEL_LAYOUT_BACKUP_FILE = @"common\config\_backUp\HCL_ODA_TestPAD_AppLayout.xml";

    /// <summary>
    ///
    /// </summary>
    public AppSettings AppSettings { get; set; }
    public AppSettings AppSettingsHCL { get; set; }

    /// <summary>
    ///
    /// </summary>
    public void LoadAppSettings()
    {
        CheckAppSettingsFolder();
        AppSettings = SettingsSerializer<AppSettings>.Load(PathResolver.GetTargetPathUsingRelativePath(APP_SETTINGS_FILE));
        AppSettingsHCL = SettingsSerializer<AppSettings>.Load(PathResolver.GetTargetPathUsingRelativePath(HCL_APP_SETTINGS_FILE));
        if (AppSettings == null)
        {
            CompareSettings("HCL");
        }
        else
        {
            CompareSettings("Initial");
        }
    }

    private void CheckAppSettingsFolder()
    {
        var appSettingsFolder = PathResolver.GetTargetPathUsingRelativePath(APP_SETTINGS_FOLDER);
        if (!Directory.Exists(appSettingsFolder))
        {
            Directory.CreateDirectory(appSettingsFolder);
            var backUpAppDockPanelLayoutFile = RecursivePathFinder.SearchFileFolderPath(APP_DOCK_PANEL_LAYOUT_BACKUP_FILE);
            var appDockLayoutFile = PathResolver.GetTargetPathUsingRelativePath(APP_DOCKPANELLAYOUT_FILE);
            File.Copy(backUpAppDockPanelLayoutFile, appDockLayoutFile);
        }
    }

    public void SaveAppSettings()
    {
        SettingsSerializer<AppSettings>.Save(AppSettings, PathResolver.GetTargetPathUsingRelativePath(APP_SETTINGS_FILE));
    }

    public void CompareSettings(string itemValue)
    {
        if (itemValue == SettingsMode.HCL.ToString())
        {
            var hclSettings = AppSettingsHCL with { SettingsMode = SettingsMode.HCL };
            AppSettings = hclSettings;
        }
        else
        {
            if (Equals(AppSettings, AppSettingsHCL))
            {
                var hclSettings = AppSettingsHCL with { SettingsMode = SettingsMode.HCL };
                AppSettings = hclSettings;
            }
            else
            {
                var customSettings = AppSettings with { SettingsMode = SettingsMode.Custom };
                AppSettings = customSettings;
            }
        }
    }
    private bool Equals(AppSettings currentSettings, AppSettings hclSettings)
    {
        string currentSettingsStr = JsonConvert.SerializeObject(currentSettings)[..^2]; 
        string hclSettingsStr = JsonConvert.SerializeObject(hclSettings)[..^2];
        return currentSettingsStr == hclSettingsStr;
    }
}
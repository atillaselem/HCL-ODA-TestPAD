using System.IO;
using HCL_ODA_TestPAD.Utility;

namespace HCL_ODA_TestPAD.Settings;

public class SettingsLoader
{
    public const string APP_SETTINGS_FOLDER = @"common\config";
    public const string APP_SETTINGS_FILE = @"common\config\HCL_ODA_TestPAD_AppSettings.json";
    public const string APP_SETTINGS_BACKUP_FILE = @"common\config\_backUp\HCL_ODA_TestPAD_AppSettings.json";
    public const string APP_DOCKPANELLAYOUT_FILE = @"common\config\HCL_ODA_TestPAD_AppLayout.xml";
    public const string APP_DOCK_PANEL_LAYOUT_BACKUP_FILE = @"common\config\_backUp\HCL_ODA_TestPAD_AppLayout.xml";

    /// <summary>
    ///
    /// </summary>
    public AppSettings AppSettings { get; set; }

    /// <summary>
    ///
    /// </summary>
    public void LoadAppSettings()
    {
        CheckAppSettingsFolder();
        AppSettings = SettingsSerializer<AppSettings>.Load(PathResolver.GetTargetPathUsingRelativePath(APP_SETTINGS_FILE));
    }

    private void CheckAppSettingsFolder()
    {
        var appSettingsFolder = PathResolver.GetTargetPathUsingRelativePath(APP_SETTINGS_FOLDER);
        if (!Directory.Exists(appSettingsFolder))
        {
            Directory.CreateDirectory(appSettingsFolder);
            var backUpAppSettingsFile = RecursivePathFinder.SearchFileFolderPath(APP_SETTINGS_BACKUP_FILE);
            var appSettingsFile = PathResolver.GetTargetPathUsingRelativePath(APP_SETTINGS_FILE);
            File.Copy(backUpAppSettingsFile, appSettingsFile);
            var backUpAppDockPanelLayoutFile = RecursivePathFinder.SearchFileFolderPath(APP_DOCK_PANEL_LAYOUT_BACKUP_FILE);
            var appDockLayoutFile = PathResolver.GetTargetPathUsingRelativePath(APP_DOCKPANELLAYOUT_FILE);
            File.Copy(backUpAppDockPanelLayoutFile, appDockLayoutFile);
        }
    }

    /// <summary>
    ///
    /// </summary>
    public void SaveAppSettings()
    {
        SettingsSerializer<AppSettings>.Save(AppSettings, PathResolver.GetTargetPathUsingRelativePath(APP_SETTINGS_FILE));
    }
}
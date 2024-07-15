using System.IO;
using HCL_ODA_TestPAD.Utility;

namespace HCL_ODA_TestPAD.Settings;

public class SettingsProvider : ISettingsProvider
{
    public const string AppSettingsFolder = @"common\config";
    public const string AppSettingsFile = @"common\config\HCL_ODA_TestPAD_AppSettings.json";
    public const string HclAppSettingsFile = @"common\config\HCL_ODA_TestPAD_AppSettings_HCL.json";
    public const string AppSettingsBackupFile = @"common\config\_backUp\HCL_ODA_TestPAD_AppSettings.json";
    public const string AppDockpanellayoutFile = @"common\config\HCL_ODA_TestPAD_AppLayout.xml";
    public const string AppDockPanelLayoutBackupFile = @"common\config\_backUp\HCL_ODA_TestPAD_AppLayout.xml";

    public SettingsProvider()
    {
        _appSettingsHcl = SettingsSerializer<AppSettings>.Load(PathResolver.GetTargetPathUsingRelativePath(HclAppSettingsFile));
        LoadSettings();
    }
    public AppSettings AppSettings { get; set; }

    private readonly AppSettings _appSettingsHcl;

    /// <summary>
    ///
    /// </summary>
    private void LoadSettings()
    {
        CheckAppSettingsFolder();
        AppSettings = SettingsSerializer<AppSettings>.Load(PathResolver.GetTargetPathUsingRelativePath(AppSettingsFile));
        if (AppSettings == null)
        {
            AppSettings = _appSettingsHcl with { SettingsMode = SettingsMode.Hcl, CadFileUnit = _appSettingsHcl.CadFileUnit };
        }
    }

    private void CheckAppSettingsFolder()
    {
        var appSettingsFolder = PathResolver.GetTargetPathUsingRelativePath(AppSettingsFolder);
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
        SettingsSerializer<AppSettings>.Save(appSettings, PathResolver.GetTargetPathUsingRelativePath(AppSettingsFile));
    }

    public AppSettings CompareSettings(AppSettings appSettings, string itemValue)
    {
        // Reset settings by selecting Mode : HCL
        if (itemValue == SettingsMode.Hcl.ToString())
        {
            return _appSettingsHcl with { CadFileUnit = appSettings.CadFileUnit };
        }

        // If all settings set back to equal HCL return by only changing Mode : HCL
        var tempSettings = appSettings with { SettingsMode = SettingsMode.Hcl };
        if (tempSettings == _appSettingsHcl)
        {
            return _appSettingsHcl with {}; // every property was copied
        }
        
        //If any change of settings change Mode : Custom
        return appSettings with { SettingsMode = SettingsMode.Custom };
    }
}
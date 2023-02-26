using System;

namespace HCL_ODA_TestPAD.Settings;

public class TestPADSettings : ISettingsProvider
{
    private readonly SettingsLoader _mSettingsLoader;
    public event Action<string, object> OneOfTheSettingsChanged;
    public IAppSettings AppSettings => _mSettingsLoader.AppSettings;

    public TestPADSettings()
    {
        _mSettingsLoader = new SettingsLoader();
        LoadSettings();
    }
    private void LoadSettings()
    {
        _mSettingsLoader.LoadAppSettings();
    }
    public void SaveSettings()
    {
        _mSettingsLoader.SaveAppSettings();
    }
    public void OnDispatchChange(string itemName, object itemValue)
    {
        OneOfTheSettingsChanged?.Invoke(itemName, itemValue);
    }
}
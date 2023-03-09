using HCL_ODA_TestPAD.Mvvm.Events;
using System;
using Prism.Events;

namespace HCL_ODA_TestPAD.Settings;

public class TestPADSettings : ISettingsProvider
{
    private readonly IEventAggregator _eventAggregator;
    private readonly SettingsLoader _mSettingsLoader;
    public event Action<string, object> OneOfTheSettingsChanged;
    public IAppSettings AppSettings => _mSettingsLoader.AppSettings;

    public TestPADSettings(IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;
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
    public void OnDispatchChange(string itemName, string itemValue)
    {
        _mSettingsLoader.CompareSettings(itemValue);
        _eventAggregator.GetEvent<SettingsUpdateEvent>().Publish(_mSettingsLoader.AppSettings);
        OneOfTheSettingsChanged?.Invoke(itemName, itemValue);

    }
}
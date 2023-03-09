using System;

namespace HCL_ODA_TestPAD.Settings
{
    public interface ISettingsProvider
    {
        //void LoadSettings();
        void SaveSettings();
        IAppSettings AppSettings { get; }
        event Action<string, object> OneOfTheSettingsChanged;
        void OnDispatchChange(string itemName, string itemValue);
    }
}

using System;

namespace HCL_ODA_TestPAD.Settings
{
    public interface ISettingsProvider
    {
        AppSettings CompareSettings(AppSettings appSettings, string itemValue);
        void SaveSettings(AppSettings appSettings);
        AppSettings AppSettings { get; }
    }
}

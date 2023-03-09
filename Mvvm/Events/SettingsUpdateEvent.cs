using HCL_ODA_TestPAD.Settings;
using Prism.Events;

namespace HCL_ODA_TestPAD.Mvvm.Events;

public class SettingsUpdateEvent : PubSubEvent<AppSettings> {}
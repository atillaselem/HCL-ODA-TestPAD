using HCL_ODA_TestPAD.HCL;
using HCL_ODA_TestPAD.Services;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace HCL_ODA_TestPAD.Settings;

public interface IServiceFactory
{
    IEventAggregator EventSrv { get; }
    IMessageDialogService MessageSrv { get; }
    IConsoleService ConsoleSrv { get; }
    ISettingsProvider SettingsSrv { get; }
    AppSettings AppSettings { get; set; }
    ILogger<TestPadLogger> Logger { get; set; }
}
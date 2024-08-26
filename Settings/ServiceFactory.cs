using HCL_ODA_TestPAD.HCL;
using HCL_ODA_TestPAD.HCL.Profiler;
using HCL_ODA_TestPAD.Services;
using Microsoft.Extensions.Logging;
using Prism.Events;
using ILogger = Serilog.ILogger;

namespace HCL_ODA_TestPAD.Settings;

public class ServiceFactory : IServiceFactory
{
    public ServiceFactory(IEventAggregator eventAggregator,
        IMessageDialogService messageDialogService,
        IConsoleService consoleService,
        ISettingsProvider settingsProvider, ILogger<TestPadLogger> logger)
    {
        EventSrv = eventAggregator;
        MessageSrv = messageDialogService;
        ConsoleSrv = consoleService;
        SettingsSrv = settingsProvider;
        AppSettings = settingsProvider.AppSettings;
        Logger = logger;
    }
    public IEventAggregator EventSrv { get; }
    public IMessageDialogService MessageSrv { get; }
    public IConsoleService ConsoleSrv { get; }
    public ISettingsProvider SettingsSrv { get; }
    public AppSettings AppSettings { get; set; }
    public ILogger<TestPadLogger> Logger { get; set; }
}
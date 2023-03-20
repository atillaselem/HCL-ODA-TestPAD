using HCL_ODA_TestPAD.Services;
using Prism.Events;

namespace HCL_ODA_TestPAD.Settings;

public class ServiceFactory : IServiceFactory
{
    public ServiceFactory(IEventAggregator eventAggregator,
        IMessageDialogService messageDialogService,
        IConsoleService consoleService,
        ISettingsProvider settingsProvider)
    {
        EventSrv = eventAggregator;
        MessageSrv = messageDialogService;
        ConsoleSrv = consoleService;
        SettingsSrv = settingsProvider;
        AppSettings = settingsProvider.AppSettings;
    }
    public IEventAggregator EventSrv { get; }
    public IMessageDialogService MessageSrv { get; }
    public IConsoleService ConsoleSrv { get; }
    public ISettingsProvider SettingsSrv { get; }
    public AppSettings AppSettings { get; set; }
}
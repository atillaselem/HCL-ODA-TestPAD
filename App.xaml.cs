using HCL_ODA_TestPAD.HCL;
using HCL_ODA_TestPAD.Infrastructure;
using HCL_ODA_TestPAD.Services;
using HCL_ODA_TestPAD.Settings;
using HCL_ODA_TestPAD.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace HCL_ODA_TestPAD;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    static App()
    {
        AssemblyResolverHelper.Initialize(new[] {"libs"});
    }
    private async void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        var exception = (e.Exception.InnerException != null
            ? e.Exception.InnerException.Message
            : e.Exception.Message);
        var messageText = "Unexpected error occured. Please inform the HCL-ODA-TestPAD Admin about this error."
                          + Environment.NewLine + exception;
        await MessageDialogService.ShowInfoDialogAsync(messageText, "Unexpected error");
        //ConsoleService?.WriteL(exception);
        e.Handled = true;
        Environment.FailFast(messageText);
    }

    public IConsoleService ConsoleService { get; private set; }
    public IServiceProvider ServiceProvider { get; private set; }
    public IServiceCollection ServiceCollection { get; private set; }

    public IConfiguration Configuration { get; private set; }
    public IMessageDialogService MessageDialogService { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        BuildServicesAndConfiguration();
        ServiceProvider = Bootstrapper.Bootstrap(ServiceCollection);
        ShowMainWindow();
    }
    
    private void ShowMainWindow()
    {
        ConsoleService = ServiceProvider.GetRequiredService<IConsoleService>();
        MessageDialogService = ServiceProvider.GetRequiredService<IMessageDialogService>();
        var settingsProvider = ServiceProvider.GetRequiredService<ISettingsProvider>();
        Window mainWindow;
        if (settingsProvider.AppSettings.AppLayout == AppLayout.Default )
        {
            mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        }
        else
        {
            mainWindow = ServiceProvider.GetRequiredService<MainView>(); //Dockable Layout
        }
        mainWindow.Show();
        mainWindow.Activate();
    }

    private void BuildServicesAndConfiguration()
    {

        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        Configuration = builder.Build();
        ConfigureServices();
    }

    private void ConfigureServices()
    {
        ServiceCollection = new ServiceCollection();
        ServiceCollection.Configure<TabbedCadModelViewSettings>(Configuration.GetSection(nameof(TabbedCadModelViewSettings)));
        ServiceCollection.AddSingleton(Configuration);
        ConfigureSerilog();
    }


    private void ConfigureSerilog()
    {
        var serilogLogger = new LoggerConfiguration()
            .ReadFrom.Configuration(Configuration)
            .CreateLogger();

        ServiceCollection.AddLogging(x =>
        {
            x.AddSerilog(logger: serilogLogger, dispose: true);
        });
    }
}

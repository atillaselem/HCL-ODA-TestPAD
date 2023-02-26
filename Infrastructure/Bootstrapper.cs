using Autofac;
using Autofac.Extensions.DependencyInjection;
using HCL_ODA_TestPAD.Services;
using HCL_ODA_TestPAD.ViewModels;
using HCL_ODA_TestPAD.Views;
using Microsoft.Extensions.DependencyInjection;
using Prism.Events;
using System;
using HCL_ODA_TestPAD.Settings;

namespace HCL_ODA_TestPAD.Infrastructure;

public class Bootstrapper
{
    public static IServiceProvider Bootstrap(IServiceCollection serviceCollection)
    {
        var builder = new ContainerBuilder();

        builder.Populate(serviceCollection);

        //AddDbContext(builder);
        //builder.RegisterType<FibexDbContext>().AsSelf();
        builder.RegisterType<EventAggregator>().As<IEventAggregator>().SingleInstance();
        builder.RegisterType<ConsoleService>().As<IConsoleService>().SingleInstance();
        builder.RegisterType<MessageDialogService>().As<IMessageDialogService>();
        builder.RegisterType<MainView>().AsSelf();
        builder.RegisterType<MainViewModel>().AsSelf();
        builder.RegisterType<MainWindow>().AsSelf();

        builder.RegisterType<AppMenuViewModel>().AsSelf();
        builder.RegisterType<AppAvalonDockViewModel>().AsSelf();
        builder.RegisterType<AppStatusBarViewModel>().AsSelf();
        builder.RegisterType<OdaDatabaseExplorerViewModel>().AsSelf();
        builder.RegisterType<TestPADSettingsViewModel>().AsSelf();
        builder.RegisterType<TabbedCadModelViewModel>().AsSelf();
        builder.RegisterType<AppMonitorViewModel>().AsSelf();
        builder.RegisterType<OverlayViewModel>().AsSelf();
        builder.RegisterType<OdaMenuViewModel>().AsSelf();

        //builder.RegisterType<HclCadImageViewModel>().Keyed<ICadImageTabViewModel>(nameof(HclCadImageViewModel)).SingleInstance();
        //builder.RegisterType<HclDwgCadImageViewModel>().Keyed<ICadImageTabViewModel>(nameof(HclDwgCadImageViewModel)).SingleInstance(); ;
        //builder.RegisterType<HclIfcCadImageViewModel>().Keyed<ICadImageTabViewModel>(nameof(HclIfcCadImageViewModel)).SingleInstance();
        //builder.RegisterType<HclCadImageView>().AsSelf();

        builder.RegisterType<TestPADSettings>().As<ISettingsProvider>().SingleInstance();

        var container = builder.Build();
        return new AutofacServiceProvider(container);
    }

}

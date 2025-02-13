﻿using Autofac;
using Autofac.Extensions.DependencyInjection;
using HCL_ODA_TestPAD.Services;
using HCL_ODA_TestPAD.ViewModels;
using HCL_ODA_TestPAD.Views;
using Microsoft.Extensions.DependencyInjection;
using Prism.Events;
using System;
using HCL_ODA_TestPAD.Settings;
using HCL_ODA_TestPAD.ViewModels.Base;
using HCL_ODA_TestPAD.HCL.Profiler;
using Autofac.Extras.CommonServiceLocator;
using CommonServiceLocator;
using HCL_ODA_TestPAD.HCL;

namespace HCL_ODA_TestPAD.Infrastructure;

public class Bootstrapper
{
    public static IServiceProvider Bootstrap(IServiceCollection serviceCollection)
    {
        var builder = new ContainerBuilder();

        builder.Populate(serviceCollection);

        builder.RegisterType<EventAggregator>().As<IEventAggregator>().SingleInstance();
        builder.RegisterType<ConsoleService>().As<IConsoleService>().SingleInstance();
        builder.RegisterType<MessageDialogService>().As<IMessageDialogService>();
        builder.RegisterType<MainViewModel>().AsSelf();
        builder.RegisterType<MainView>().AsSelf();

        builder.RegisterType<AppMenuViewModel>().AsSelf();
        builder.RegisterType<AppAvalonDockViewModel>().AsSelf();
        builder.RegisterType<AppStatusBarViewModel>().AsSelf();
        builder.RegisterType<OdaDatabaseExplorerViewModel>().AsSelf();
        builder.RegisterType<TestPadSettingsViewModel>().AsSelf();
        builder.RegisterType<TabPagedCadImageViewModel>().AsSelf();
        builder.RegisterType<AppMonitorViewModel>().AsSelf();
        builder.RegisterType<OverlayViewModel>().AsSelf();
        builder.RegisterType<OdaMenuViewModel>().AsSelf();

        builder.RegisterType<HclCadImageViewModel>().Keyed<ICadImageTabViewModel>(DeviceType.BitmapDevice);

        builder.RegisterType<SettingsProvider>().As<ISettingsProvider>().SingleInstance();
        builder.RegisterType<MainWindowViewModel>().AsSelf();
        builder.RegisterType<MainWindow>().AsSelf();
        builder.RegisterType<TestPadLogger>().AsSelf();
        builder.RegisterType<ServiceFactory>().As<IServiceFactory>().SingleInstance();
        builder.RegisterType<TestPadSettings>().AsSelf();

        builder.RegisterType<CadLogProfiler>().AsSelf();
        builder.RegisterType<CadFpsProfiler>().AsSelf();

        // Perform registrations and build the container.
        var container = builder.Build();
        // Set the service locator to an AutofacServiceLocator.
        var csl = new AutofacServiceLocator(container);
        ServiceLocator.SetLocatorProvider(() => csl);
        return csl;
    }

}

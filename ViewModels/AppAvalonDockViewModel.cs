using HCL_ODA_TestPAD.Mvvm;
using HCL_ODA_TestPAD.Services;
using HCL_ODA_TestPAD.Settings;
using HCL_ODA_TestPAD.Utility;
using Prism.Events;
using System;
using System.IO;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout.Serialization;

namespace HCL_ODA_TestPAD.ViewModels
{
    public class AppAvalonDockViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IConsoleService _consoleService;
        public OdaDatabaseExplorerViewModel OdaDatabaseExplorerViewModel { get; }
        public AppMonitorViewModel AppMonitorViewModel { get; }
        public TestPADSettingsViewModel TestPADSettingsViewModel { get; }
        public TabbedCadModelViewModel TabbedCadModelViewModel { get; }
        public OverlayViewModel OverlayViewModel { get; }
        public OdaMenuViewModel OdaMenuViewModel { get; }
        private readonly ISettingsProvider _settingsProvider;

        public AppAvalonDockViewModel(OdaDatabaseExplorerViewModel odaDatabaseExplorerViewModel,
            TestPADSettingsViewModel testPADSettingsViewModel,
            TabbedCadModelViewModel tabbedCadModelViewModel,   
            AppMonitorViewModel appMonitorViewModel,
            OverlayViewModel overlayViewModel,
            OdaMenuViewModel odaMenuViewModel,
            IEventAggregator eventAggregator,
            IConsoleService consoleService,
            ISettingsProvider settingsProvider)
        {
            OdaDatabaseExplorerViewModel = odaDatabaseExplorerViewModel;
            TestPADSettingsViewModel = testPADSettingsViewModel;
            TabbedCadModelViewModel = tabbedCadModelViewModel;
            AppMonitorViewModel = appMonitorViewModel;
            OverlayViewModel = overlayViewModel;
            OdaMenuViewModel = odaMenuViewModel;
            _eventAggregator = eventAggregator;
            _consoleService = consoleService;
            _settingsProvider = settingsProvider;
        }

        public IConsoleService ConsoleService => _consoleService;
        public void SaveLayout(Func<DockingManager> dockingManagerFactory)
        {
            if (_settingsProvider.AppSettings.SaveDockLayout)
            {
                var layoutFilePath = PathResolver.GetTargetPathUsingRelativePath(SettingsLoader.APP_DOCKPANELLAYOUT_FILE);
                using var writer = new StreamWriter(layoutFilePath);
                var layoutSerializer = new XmlLayoutSerializer(dockingManagerFactory());
                layoutSerializer.Serialize(writer);
            }
        }
        public void LoadDockLayout(Func<DockingManager> dockingManagerFactory)
        {
            var layoutFilePath = PathResolver.GetTargetPathUsingRelativePath(SettingsLoader.APP_DOCKPANELLAYOUT_FILE);
            if (File.Exists(layoutFilePath))
            {
                using var reader = new StreamReader(layoutFilePath);
                var layoutSerializer = new XmlLayoutSerializer(dockingManagerFactory());
                layoutSerializer.Deserialize(reader);
            }
        }
    }
}

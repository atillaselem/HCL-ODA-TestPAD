using HCL_ODA_TestPAD.Mvvm;
using HCL_ODA_TestPAD.Services;
using Prism.Events;

namespace HCL_ODA_TestPAD.ViewModels
{
    public class OdaCadImageViewModel : BindableBase
    {
        public OverlayViewModel OverlayViewModel { get; }
        private readonly IEventAggregator _eventAggregator;
        private readonly IConsoleService _consoleService;

        public OdaCadImageViewModel(OdaDatabaseExplorerViewModel odaDatabaseExplorerViewModel,
            AppMonitorViewModel appMonitorViewModel,
            OverlayViewModel overlayViewModel,
            IEventAggregator eventAggregator,
            IConsoleService consoleService)
        {
            OverlayViewModel = overlayViewModel;
            _eventAggregator = eventAggregator;
            _consoleService = consoleService;
        }
    }
}

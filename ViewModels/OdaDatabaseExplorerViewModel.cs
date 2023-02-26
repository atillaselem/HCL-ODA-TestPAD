using HCL_ODA_TestPAD.Mvvm;
using Prism.Events;

namespace HCL_ODA_TestPAD.ViewModels
{
    public class OdaDatabaseExplorerViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        public OverlayViewModel OverlayViewModel { get; }

        public OdaDatabaseExplorerViewModel(IEventAggregator eventAggregator,
            OverlayViewModel overlayViewModel)
        {
            _eventAggregator = eventAggregator;
            OverlayViewModel = overlayViewModel;
            OverlayViewModel.Title = "Fibex is loading..";
        }

    }
}

using Autofac.Features.Indexed;
using HCL_ODA_TestPAD.Mvvm;
using HCL_ODA_TestPAD.Mvvm.Events;
using HCL_ODA_TestPAD.Services;
using HCL_ODA_TestPAD.Settings;
using HCL_ODA_TestPAD.ViewModels.Base;
using Prism.Events;
using System.Collections.ObjectModel;
using System.Linq;

namespace HCL_ODA_TestPAD.ViewModels
{
    public class CadImageViewModelWrapper
    {
        public ICadImageTabViewModel CurrentTabViewModel { get; set;}
    }
    public class TabPagedCadImageViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IMessageDialogService _messageDialogService;
        private readonly IConsoleService _consoleService;
        private readonly ISettingsProvider _settingsProvider;
        private readonly TabbedCadModelViewSettings _tabbedCadModelViewSettings;
        private readonly IIndex<DeviceType, ICadImageTabViewModel> _cadModelTabViewCreator;
        public ObservableCollection<CadImageViewModelWrapper> CadImageTabViewModels { get; }

        public TabPagedCadImageViewModel(
            IEventAggregator eventAggregator,
            IMessageDialogService messageDialogService,
            IIndex<DeviceType, ICadImageTabViewModel> cadModelTabViewCreator,
            IConsoleService consoleService,
            ISettingsProvider settingsProvider)
        {
            _eventAggregator = eventAggregator;
            _messageDialogService = messageDialogService;
            _cadModelTabViewCreator = cadModelTabViewCreator;
            _consoleService = consoleService;
            _settingsProvider = settingsProvider;
            CadImageTabViewModels = new ObservableCollection<CadImageViewModelWrapper>();
            SubscribeEvents();

        }

        private void SubscribeEvents()
        {
            _eventAggregator.GetEvent<OpenCadModelTabViewEvent>()
                .Subscribe(OnOpenCadModelTabViewEvent);
            _eventAggregator.GetEvent<CloseCadModelTabViewEvent>()
                .Subscribe(OnCloseCadModelTabViewEvent);
            _eventAggregator.GetEvent<OdaMenuCommandClickEvent>()
                .Subscribe(OnOdaMenuCommandClickEvent);
            _eventAggregator.GetEvent<UnLoadTabViewEvent>().Subscribe(OnUnLoadTabViewEvent);
        }

        private void OnOdaMenuCommandClickEvent(OdaMenuCommandClickEventArg odaMenuCommandArg)
        {
            if (SelectedCadImageTabViewModel == null) return;
            switch (odaMenuCommandArg.OdaEventType)
            {
                case OdaEventType.Panning : SelectedCadImageTabViewModel.CurrentTabViewModel.OnPanClicked(); break;
                case OdaEventType.Orbitting: SelectedCadImageTabViewModel.CurrentTabViewModel.OnOrbitClicked(); break;
                case OdaEventType.SetZoom: SelectedCadImageTabViewModel.CurrentTabViewModel.OnZoomClicked(odaMenuCommandArg.eZoomType); break;
                case OdaEventType.Set3DView: SelectedCadImageTabViewModel.CurrentTabViewModel.OnSet3DViewClicked(odaMenuCommandArg.e3DViewType); break;
                case OdaEventType.SetRender: SelectedCadImageTabViewModel.CurrentTabViewModel.OnRenderModeClicked(odaMenuCommandArg.eRenderModeType); break;
                case OdaEventType.SetProjection: SelectedCadImageTabViewModel.CurrentTabViewModel.OnSetProjectionClicked(odaMenuCommandArg.eProjectionType); break;
                case OdaEventType.SetRegen: SelectedCadImageTabViewModel.CurrentTabViewModel.OnRegenModeClicked(odaMenuCommandArg.eRegenModeType); break;
                case OdaEventType.RegenView: SelectedCadImageTabViewModel.CurrentTabViewModel.OnRegenViewClicked(); break;
            };
        }

        private void OnUnLoadTabViewEvent()
        {
            foreach (var tabViewModel in CadImageTabViewModels)
            {
                var currentTabViewModel = tabViewModel.CurrentTabViewModel;
                currentTabViewModel.CloseTabView();
            }
        }

        private CadImageViewModelWrapper _selectedDetailViewModel;
        public CadImageViewModelWrapper SelectedCadImageTabViewModel
        {
            get => _selectedDetailViewModel;
            set => SetProperty(ref _selectedDetailViewModel, value);
        }

        private void OnCloseCadModelTabViewEvent(CloseCadModelTabViewEventArgs args)
        {
            var cadImageTabViewModel = CadImageTabViewModels
                .SingleOrDefault(vm => vm.CurrentTabViewModel.TabItemTitle == args.CadModelTabViewKey);

            if (cadImageTabViewModel != null)
            {
                CadImageTabViewModels.Remove(cadImageTabViewModel);                
            }
        }

        private void OnOpenCadModelTabViewEvent(OpenCadModelTabViewEventArgs args)
        {
            var filePath = args.ViewModelFilePath;
            var viewTitle = args.ViewModelKey;

            var currentTabViewModel = _cadModelTabViewCreator[DeviceType.BitmapDevice];
            currentTabViewModel.TabItemTitle = viewTitle;
            currentTabViewModel.CadImageFilePath = filePath;
            var wrapper = new CadImageViewModelWrapper()
            {
                CurrentTabViewModel = currentTabViewModel
            };
            CadImageTabViewModels.Add(wrapper);
            SelectedCadImageTabViewModel = wrapper;
        }

        internal void TabPageSelectionChanged(int selectedIndex)
        {
            _eventAggregator.GetEvent<TabPageSelectionChangedEvent>().Publish(selectedIndex);
        }
    }
}

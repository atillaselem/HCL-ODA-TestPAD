using Autofac.Features.Indexed;
using HCL_ODA_TestPAD.Mvvm;
using HCL_ODA_TestPAD.Mvvm.Events;
using HCL_ODA_TestPAD.Settings;
using HCL_ODA_TestPAD.ViewModels.Base;
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
        private readonly IServiceFactory _serviceFactory;
        private readonly IIndex<DeviceType, ICadImageTabViewModel> _cadModelTabViewCreator;
        public ObservableCollection<CadImageViewModelWrapper> CadImageTabViewModels { get; }

        public TabPagedCadImageViewModel(IServiceFactory serviceFactory,
            IIndex<DeviceType, ICadImageTabViewModel> cadModelTabViewCreator)
        {
            _serviceFactory = serviceFactory;
            _cadModelTabViewCreator = cadModelTabViewCreator;
            CadImageTabViewModels = new ObservableCollection<CadImageViewModelWrapper>();
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            _serviceFactory.EventSrv.GetEvent<OpenCadModelTabViewEvent>()
                .Subscribe(OnOpenCadModelTabViewEvent);
            _serviceFactory.EventSrv.GetEvent<CloseCadModelTabViewEvent>()
                .Subscribe(OnCloseCadModelTabViewEvent);
            _serviceFactory.EventSrv.GetEvent<OdaMenuCommandClickEvent>()
                .Subscribe(OnOdaMenuCommandClickEvent);
            _serviceFactory.EventSrv.GetEvent<UnLoadTabViewEvent>().Subscribe(OnUnLoadTabViewEvent);
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
            _serviceFactory.EventSrv.GetEvent<TabPageSelectionChangedEvent>().Publish(selectedIndex);
        }
    }
}

using HCL_ODA_TestPAD.Functional;
using HCL_ODA_TestPAD.Mvvm;
using HCL_ODA_TestPAD.Mvvm.Events;
using HCL_ODA_TestPAD.Services;
using HCL_ODA_TestPAD.Settings;
using HCL_ODA_TestPAD.ViewModels.Base;
using Prism.Events;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace HCL_ODA_TestPAD.ViewModels
{
    public class TabbedCadModelViewModel : BindableBase
    {
        public OverlayViewModel OverlayViewModel { get; }
        private readonly IEventAggregator _eventAggregator;
        private readonly IMessageDialogService _messageDialogService;
        private readonly IConsoleService _consoleService;
        private readonly ISettingsProvider _settingsProvider;
        private readonly TabbedCadModelViewSettings _tabbedCadModelViewSettings;
        //private readonly IIndex<string, ICadImageTabViewModel> _cadModelTabViewCreator;
        public ObservableCollection<HclCadImageViewModel> CadImageTabViewModels { get; }

        public TabbedCadModelViewModel(
            OverlayViewModel overlayViewModel,
            IEventAggregator eventAggregator,
            IMessageDialogService messageDialogService,
            //IOptions<TabbedCadModelViewSettings> tabbedCadModelViewSettings,
            //IIndex<string, ICadImageTabViewModel> cadModelTabViewCreator,
            IConsoleService consoleService,
            ISettingsProvider settingsProvider)
        {
            OverlayViewModel = overlayViewModel;
            _eventAggregator = eventAggregator;
            _messageDialogService = messageDialogService;
            //_cadModelTabViewCreator = cadModelTabViewCreator;
            //_tabbedCadModelViewSettings = tabbedCadModelViewSettings.Value;
            _consoleService = consoleService;
            _settingsProvider = settingsProvider;
            CadImageTabViewModels = new ObservableCollection<HclCadImageViewModel>();
            SubscribeEvents();

        }

        private void SubscribeEvents()
        {
            _eventAggregator.GetEvent<OpenCadModelTabViewEvent>()
                .Subscribe(OnOpenCadModelTabViewEvent);
            _eventAggregator.GetEvent<CloseCadModelTabViewEvent>()
                .Subscribe(OnCloseCadModelTabViewEvent);
            _eventAggregator.GetEvent<PanCommandClickedEvent>()
                .Subscribe(OnPanCommandClickedEvent);
            _eventAggregator.GetEvent<OrbitCommandClickedEvent>()
                .Subscribe(OnOrbitCommandClickedEvent);
            _eventAggregator.GetEvent<UnLoadTabViewEvent>().Subscribe(OnUnLoadTabViewEvent);
        }
        private void OnUnLoadTabViewEvent()
        {
            SelectedCadImageTabViewModel?.CloseTabView();
            OnCloseCadModelTabViewEvent(new CloseCadModelTabViewEventArgs
            {
                CadModelTabViewKey = SelectedCadImageTabViewModel?.TabItemTitle
            });
        }
        private void OnPanCommandClickedEvent()
        {
            SelectedCadImageTabViewModel.OnPanClicked();
        }
        private void OnOrbitCommandClickedEvent()
        {
            SelectedCadImageTabViewModel.OnOrbitClicked();
        }

        private ICadImageTabViewModel _selectedDetailViewModel;
        public ICadImageTabViewModel SelectedCadImageTabViewModel
        {
            get => _selectedDetailViewModel;
            set => SetProperty(ref _selectedDetailViewModel, value);
        }

        private void OnCloseCadModelTabViewEvent(CloseCadModelTabViewEventArgs args)
        {
            var cadImageTabViewModel = CadImageTabViewModels
                .SingleOrDefault(vm => vm.TabItemTitle == args.CadModelTabViewKey);

            if (cadImageTabViewModel != null)
            {
                CadImageTabViewModels.Remove(cadImageTabViewModel);                
            }
        }

        private async void OnOpenCadModelTabViewEvent(OpenCadModelTabViewEventArgs args)
        {
            OnUnLoadTabViewEvent();
            var filePath = args.ViewModelFilePath;
            var viewTitle = args.ViewModelKey;

            //if (_tabbedCadModelViewSettings.ViewModelBag.TryGetValue(viewType,
            //                    out string configViewModel))
            //{
            //    OverlayViewModel.IsLoading = true;
            //    OverlayViewModel.Title = viewTitle + " is loading..";
            //}
            OverlayViewModel.IsLoading = true;
            OverlayViewModel.Title = viewTitle + " is loading..";
            await Task.Delay(1000);
            await Task.Factory.StartNew(
                    () => CreateCadModelTabViewModel(
                    () => CadImageTabViewModels.SingleOrDefault(vm => vm.TabItemTitle == viewTitle),
                    () =>
                        {
                            return Result.Ok<string>("HclCadImageViewModel");
                        },
                            cfgViewModelResult =>
                            {
                                var cadImageTabViewModel = new HclCadImageViewModel(
                                    _eventAggregator,
                                    _messageDialogService,
                                    _consoleService,
                                    _settingsProvider)
                                {
                                    TabItemTitle = viewTitle,
                                    CadImageFilePath = filePath
                                };
                                return cadImageTabViewModel;
                            }
                            ));
        }

        private async Task<Result> CreateCadModelTabViewModel(Func<HclCadImageViewModel> FindViewModel,
                                                 Func<Result<string>> CheckCadModelViewBag,
                                                 Func<Result<string>, HclCadImageViewModel> CadImageTabViewModelFactory)
        {
            var cadImageTabViewModel = FindViewModel();

            if (cadImageTabViewModel == null)
            {
                var configViewModelResult = CheckCadModelViewBag();
                if (configViewModelResult.IsFailure)
                {
                    _consoleService.WriteR(configViewModelResult.Value);
                    return Result.Fail(configViewModelResult.Value);
                }

                cadImageTabViewModel = CadImageTabViewModelFactory(configViewModelResult);
                try
                {
                    await cadImageTabViewModel.LoadCadModelViewAsync();
                }
                catch (Exception)
                {
                    //throw new Exception("Data Model Loading error", ex);
                    return Result.Fail("Data Model Loading error");
                }
                UiDispatcher?.BeginInvoke(new Action(() =>
                {
                    CadImageTabViewModels.Add(cadImageTabViewModel);
                    SelectedCadImageTabViewModel = cadImageTabViewModel;
                }), DispatcherPriority.Background);
            }
            else
            {
                UiDispatcher?
                    .BeginInvoke(new Action(() =>
                    {
                        SelectedCadImageTabViewModel = cadImageTabViewModel;
                    }), DispatcherPriority.Background);
            }
            OverlayViewModel.IsLoading = false;
            return Result.Ok();
        }

        public Dispatcher UiDispatcher { get; set; }
    }
}

using HCL_ODA_TestPAD.Mvvm;
using HCL_ODA_TestPAD.Mvvm.Events;
using Prism.Commands;
using Prism.Events;
using System.Windows.Input;

namespace HCL_ODA_TestPAD.ViewModels
{
    public class AppMenuViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        private const string _menuSaveDataModel = "Save Data Model (.db)";
        private const string _menuMergeAndSaveDataModel = "Merge & Save Data Model (.db)";

        public AppMenuViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<FibexFileLoadedEvent>()
                .Subscribe(OnCanSaveDataModel);

        }


        private string _mergeAndSaveMenuName = _menuSaveDataModel;
        public string MergeAndSaveMenuName
        {
            get => _mergeAndSaveMenuName;
            set => SetProperty(ref _mergeAndSaveMenuName, value);
        }

        private bool _canSaveDataModel;
        public bool CanSaveDataModel
        {
            get => _canSaveDataModel;
            set => SetProperty(ref _canSaveDataModel, value);
        }

        private void OnCanSaveDataModel(FibexFileLoadedEventArgs args)
        {
            if (args.IsMultipleFiles) MergeAndSaveMenuName = _menuMergeAndSaveDataModel;
            else MergeAndSaveMenuName = _menuSaveDataModel;

            CanSaveDataModel = true;
        }

        private DelegateCommand _viewDataModel;
        public ICommand ViewDataModelCommand => _viewDataModel ??= new DelegateCommand(OnMenuViewDataModel);
        //public ICommand ViewDataModelCommand => _viewDataModel ??= new ViewDataModelCommand(OnMenuViewDataModel);

        //private DelegateCommand _saveDataModel;
        //public ICommand SaveDataModelCommand => _saveDataModel ??=
        //    new DelegateCommand(OnMenuSaveDataModel).ObservesCanExecute(()=> CanSaveDataModel);

        private DelegateCommand _mergeDataModel;
        public ICommand MergeDataModelCommand => _mergeDataModel ??=
            new DelegateCommand(OnMenuMergeDataModel).ObservesCanExecute(() => CanSaveDataModel);

        private DelegateCommand _mergeAndSaveDataModel;
        public ICommand MergeAndSaveDataModelCommand => _mergeAndSaveDataModel ??=
            new DelegateCommand(OnMenuMergeAndSaveDataModel).ObservesCanExecute(() => CanSaveDataModel);

        //private DelegateCommand _loadFibexCommand;
        //public ICommand LoadFibexCommand => _loadFibexCommand ??=
        //    new DelegateCommand(OnMenuLoadFibex);

        //private void OnMenuSaveDataModel() => 
        //    _eventAggregator.GetEvent<SaveDataModelEvent>().Publish();

        private void OnMenuMergeDataModel() =>
        _eventAggregator.GetEvent<MergeDataModelEvent>().Publish();

        private void OnMenuMergeAndSaveDataModel() =>
            _eventAggregator.GetEvent<MergeAndSaveDataModelEvent>().Publish();

        private void OnMenuViewDataModel() =>
            _eventAggregator.GetEvent<OpenCadModelTabViewEvent>()
            .Publish(new OpenCadModelTabViewEventArgs { ViewModelKey = "DATA_MODEL" });

        //private void OnMenuLoadFibex() =>
        //    _eventAggregator.GetEvent<LoadFibexFileEvent>().Publish(new LoadFibexFileEventArgs());

    }
}

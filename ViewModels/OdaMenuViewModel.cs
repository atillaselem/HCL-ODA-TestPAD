using System.IO;
using System.Windows.Input;
using HCL_ODA_TestPAD.Mvvm;
using HCL_ODA_TestPAD.Mvvm.Events;
using HCL_ODA_TestPAD.Settings;
using HCL_ODA_TestPAD.Views;
using MEGA_MainWindow.Splash;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Events;

namespace HCL_ODA_TestPAD.ViewModels
{
    public class OdaMenuViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        private DelegateCommand _openCommand;
        public ICommand OpenCommand => _openCommand ??=new DelegateCommand(OnMenuOpenCommand, ()=> true);

        private DelegateCommand _exitCommand;
        public ICommand ExitCommand => _exitCommand ??=new DelegateCommand(OnMenuExitCommand, ()=> true);

        public IOdaMenuView OdaMenuView { get; set; }
        private readonly IAppSettings _appSettings;
        public OdaMenuViewModel(IEventAggregator eventAggregator,
        ISettingsProvider settingsProvider)
        {
            _eventAggregator = eventAggregator;
            SubscribeEvents();
            IsNavigation = true;
            _appSettings = settingsProvider.AppSettings;
        }

        private void SubscribeEvents()
        {
            _eventAggregator.GetEvent<CadModelLoadedEvent>().Subscribe(OnCadModelLoadedEvent);
            _eventAggregator.GetEvent<CloseCadModelTabViewEvent>().Subscribe(OnCloseCadModelTabViewEvent);
        }

        private void OnCloseCadModelTabViewEvent(CloseCadModelTabViewEventArgs args)
        {
            FileIsExist = false;
        }

        private void OnCadModelLoadedEvent()
        {
            FileIsExist= true;
            NavigationMenuCommand_Clicked();
        }

        private void ResetMenuFlags()
        {
            IsNavigation = false;
            IsView = false;
            IsProjection = false;
            IsStyle = false;
            IsRegen = false;
            IsDrawing = false;
            IsMarkup = false;
            IsSectioning = false;
            IsPanels = false;
            IsPanClicked= false;
            IsOrbitClicked= false;
            IsAbout = false;
        }
        public void OnMenuOpenCommand()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Open Design Visualize Stream|*.vsf|" +
                         "DWG files|*.vsfx|" +
                         "DWG files|*.dwg|" +
                         "IFC files|*.ifc|" +
                         "OBJ files|*.obj|" +
                         "STL files|*.stl|" +
                         "All Supported files|*.vsf;*.vsfx;*.dwg;*.ifc;*.obj;*.stl";

            RegistryKey key = Registry.CurrentUser.OpenSubKey("WpfVisualizeViewer_OpenDialogIndex", true);
            if (key == null)
            {
                key = Registry.CurrentUser.CreateSubKey("WpfVisualizeViewer_OpenDialogIndex");
                key.SetValue("Index", 1);
            }
            dlg.FilterIndex = (int)key.GetValue("Index");

            if (dlg.ShowDialog() != true)
                return;

            key.SetValue("Index", dlg.FilterIndex);
            key.Close();

            _eventAggregator.GetEvent<OpenCadModelTabViewEvent>()
                .Publish(
                    new OpenCadModelTabViewEventArgs
                    {
                        ViewModelFilePath = dlg.FileName,
                        ViewModelKey = Path.GetFileName(dlg.FileName)
                    });

        }
        public void OnMenuExitCommand()
        {
            _eventAggregator.GetEvent<ExitApplicationEvent>().Publish();
        }
        private bool _fileIsExist;
        public bool FileIsExist
        {
            get => _fileIsExist;
            set => SetProperty(ref _fileIsExist, value);
        }

        private bool _isNavigation = false;
        public bool IsNavigation
        {
            get => _isNavigation;
            set => SetProperty(ref _isNavigation, value);
        }

        private bool _isView;
        public bool IsView
        {
            get => _isView;
            set => SetProperty(ref _isView, value);
        }

        private bool _isProjection = false;
        public bool IsProjection
        {
            get => _isProjection;
            set => SetProperty(ref _isProjection, value);
        }

        private bool _isStyle = false;
        public bool IsStyle
        {
            get => _isStyle;
            set => SetProperty(ref _isStyle, value);
        }

        private bool _isRegen;
        public bool IsRegen
        {
            get => _isRegen;
            set => SetProperty(ref _isRegen, value);
        }

        private bool _isDrawing = false;
        public bool IsDrawing
        {
            get => _isDrawing;
            set => SetProperty(ref _isDrawing, value);
        }

        private bool _isMarkup;
        public bool IsMarkup
        {
            get => _isMarkup;
            set => SetProperty(ref _isMarkup, value);
        }

        private bool _isSectioning;
        public bool IsSectioning
        {
            get => _isSectioning;
            set => SetProperty(ref _isSectioning, value);
        }

        private bool _isPanels;
        public bool IsPanels
        {
            get => _isPanels;
            set => SetProperty(ref _isPanels, value);
        }

        private bool _isAbout;
        public bool IsAbout
        {
            get => _isAbout;
            set => SetProperty(ref _isAbout, value);
        }
        private DelegateCommand _navigationMenuCommand;
        public ICommand NavigationMenuCommand => _navigationMenuCommand ??= new DelegateCommand(NavigationMenuCommand_Clicked).ObservesProperty(() => FileIsExist);

        private void NavigationMenuCommand_Clicked()
        {
            ResetMenuFlags();
            IsNavigation = true;
            SyncContext.Post(o => OdaMenuView?.PlayNavRectAnimation(600), null);
        }
        private DelegateCommand _viewMenuCommand;
        public ICommand ViewMenuCommand => _viewMenuCommand ??= new DelegateCommand(ViewMenuCommand_Clicked).ObservesProperty(() => FileIsExist);
        private void ViewMenuCommand_Clicked()
        {
            ResetMenuFlags();
            IsView = true;
            SyncContext.Post(o => OdaMenuView?.PlayNavRectAnimation(500), null);
        }
        private DelegateCommand _projMenuCommand;
        public ICommand ProjMenuCommand => _projMenuCommand ??= new DelegateCommand(ProjMenuCommand_Clicked).ObservesProperty(() => FileIsExist);

        private void ProjMenuCommand_Clicked()
        {
            ResetMenuFlags();
            IsProjection = true;
            SyncContext.Post(o => OdaMenuView?.PlayNavRectAnimation(400), null);
        }
        private DelegateCommand _styleMenuCommand;
        public ICommand StyleMenuCommand => _styleMenuCommand ??= new DelegateCommand(StyleMenuCommand_Clicked).ObservesProperty(() => FileIsExist);

        private void StyleMenuCommand_Clicked()
        {
            ResetMenuFlags();
            IsStyle = true;
            SyncContext.Post(o => OdaMenuView?.PlayNavRectAnimation(300), null);
        }

        private DelegateCommand _regenMenuCommand;
        public ICommand RegenMenuCommand => _regenMenuCommand ??= new DelegateCommand(RegenMenuCommand_Clicked).ObservesProperty(() => FileIsExist);

        private void RegenMenuCommand_Clicked()
        {
            ResetMenuFlags();
            IsRegen = true;
            SyncContext.Post(o => OdaMenuView?.PlayNavRectAnimation(200), null);
        }
        private DelegateCommand _helpMenuCommand;
        public ICommand HelpMenuCommand => _helpMenuCommand ??= new DelegateCommand(HelpMenuCommand_Clicked).ObservesProperty(() => FileIsExist);

        private void HelpMenuCommand_Clicked()
        {
            ResetMenuFlags();
            IsAbout = true;
            SyncContext.Post(o => OdaMenuView?.PlayNavRectAnimation(100), null);
            //_eventAggregator.GetEvent<PanCommandClickedEvent>().Publish();
        }

        private DelegateCommand _markUpMenuCommand;
        public ICommand MarkupMenuCommand => _markUpMenuCommand ??= new DelegateCommand(MarkupMenuCommand_Clicked).ObservesProperty(() => FileIsExist);

        private void MarkupMenuCommand_Clicked()
        {
            ResetMenuFlags();
            IsMarkup = true;
            SyncContext.Post(o => OdaMenuView?.PlayNavRectAnimation(100), null);
        }

        private DelegateCommand _drawingMenuCommand;
        public ICommand DrawingMenuCommand => _drawingMenuCommand ??= new DelegateCommand(DrawingMenuCommand_Clicked).ObservesProperty(() => FileIsExist);

        private void DrawingMenuCommand_Clicked()
        {
            ResetMenuFlags();
            IsDrawing = true;
            SyncContext.Post(o => OdaMenuView?.PlayNavRectAnimation(0), null);
        }

        private DelegateCommand _sectioningMenuCommand;
        public ICommand SectioningMenuCommand => _sectioningMenuCommand ??= new DelegateCommand(SectioningMenuCommand_Clicked).ObservesProperty(() => FileIsExist);

        private void SectioningMenuCommand_Clicked()
        {
            ResetMenuFlags();
            IsSectioning = true;
            SyncContext.Post(o => OdaMenuView?.PlayNavRectAnimation(-100), null);
            //if (_hclIOpenGLES2Control != null)
            //    _hclIOpenGLES2Control.OnAppearSectioningPanel(true);
        }
        private DelegateCommand _panCommand;
        public ICommand PanCommand => _panCommand ??= new DelegateCommand(PanCommand_Clicked).ObservesProperty(() => FileIsExist);

        private void PanCommand_Clicked()
        {
            ResetMenuFlags();
            IsNavigation = true;
            IsPanClicked = true;
            _eventAggregator.GetEvent<PanCommandClickedEvent>().Publish();
        }
        private bool _isPanClicked;
        public bool IsPanClicked
        {
            get => _isPanClicked;
            set => SetProperty(ref _isPanClicked, value);
        }
        private bool _isOrbitClicked;
        public bool IsOrbitClicked
        {
            get => _isOrbitClicked;
            set => SetProperty(ref _isOrbitClicked, value);
        }
        private DelegateCommand _orbitCommand;
        public ICommand OrbitCommand => _orbitCommand ??= new DelegateCommand(OrbitCommand_Clicked).ObservesProperty(() => FileIsExist);

        private void OrbitCommand_Clicked()
        {
            ResetMenuFlags();
            IsNavigation = true;
            IsOrbitClicked = true;
            _eventAggregator.GetEvent<OrbitCommandClickedEvent>().Publish();
        }
        private DelegateCommand _aboutCommand;
        public ICommand AboutCommand => _aboutCommand ??= new DelegateCommand(AboutCommand_Clicked).ObservesProperty(() => FileIsExist);

        private void AboutCommand_Clicked()
        {
            //ResetMenuFlags();
            if (_appSettings.ShowAboutAnimation)
            {
                var aboutDlg = new AboutTestPAD();
                aboutDlg.Show();
            }
        }
    }
}

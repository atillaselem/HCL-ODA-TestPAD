using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using HCL_ODA_TestPAD.Mvvm;
using HCL_ODA_TestPAD.Mvvm.Events;
using HCL_ODA_TestPAD.Resources.Converters;
using HCL_ODA_TestPAD.Settings;
using HCL_ODA_TestPAD.Splash;
using HCL_ODA_TestPAD.ViewModels.Base;
using HCL_ODA_TestPAD.Views;
using Microsoft.Win32;
using ODA.Visualize.TV_Visualize;
using ODA.Visualize.TV_VisualizeTools;
using Prism.Commands;

namespace HCL_ODA_TestPAD.ViewModels
{
    public class OdaMenuViewModel : BindableBase
    {
        private readonly IServiceFactory _serviceFactory;
        private DelegateCommand _openCommand;
        public ICommand OpenCommand => _openCommand ??= new DelegateCommand(OnMenuOpenCommand, () => true);

        private DelegateCommand _exitCommand;
        public ICommand ExitCommand => _exitCommand ??= new DelegateCommand(OnMenuExitCommand, () => true);

        public IOdaMenuView OdaMenuView { get; set; }

        public Dictionary<ButtonName, Func<ToggleButton>> ButtonFactory { get; set; }
        private int _numberOfTabPages;
        public OdaMenuViewModel(IServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;

            SubscribeEvents();
            IsNavigation = true;
        }

        private void SubscribeEvents()
        {
            _serviceFactory.EventSrv.GetEvent<CadModelLoadedEvent>().Subscribe(OnCadModelLoadedEvent);
            _serviceFactory.EventSrv.GetEvent<CloseCadModelTabViewEvent>().Subscribe(OnCloseCadModelTabViewEvent);
            _serviceFactory.EventSrv.GetEvent<TabPageSelectionChangedEvent>().Subscribe(OnTabPageSelectionChangedEvent);
        }


        private void OnTabPageSelectionChangedEvent(int selectedIndex)
        {
            ReSetMenuStates();
        }

        private void SetMenuStatesForCurrentTabPage()
        {
            foreach (var kvp in ButtonFactory)
            {
                var toggleButton = kvp.Value();
                if ((bool)toggleButton.IsChecked)
                {
                    switch (toggleButton.Name)
                    {
                        case "PanBtn":
                            PanCommand_Clicked(); break;
                        case "OrbitBtn":
                            OrbitCommand_Clicked(); break;
                        case "IsometricBtn":
                        case "PerspectiveBtn":
                            ProjectionCommand_Clicked(toggleButton.CommandParameter); break;
                        case "Wireframe2DBtn":
                        case "Wireframe3DBtn":
                        case "HiddenLineBtn":
                        case "ShadedBtn":
                        case "ShadedWithEdgesBtn":
                            RenderModeCommand_Clicked(toggleButton.CommandParameter); break;
                    }

                }
            }
            NavigationMenuCommand_Clicked();
        }

        private void ReSetMenuStates()
        {
            foreach (var kvp in ButtonFactory)
            {
                var toggleButton = kvp.Value();
                toggleButton.IsChecked = false;
            }
            NavigationMenuCommand_Clicked();
        }

        private void OnCloseCadModelTabViewEvent(CloseCadModelTabViewEventArgs args)
        {
            if(--_numberOfTabPages == 0)
            {
                FileIsExist = false;
            }
            if(ViewModelToViewConverter.DictModelView.ContainsKey(args.CadModelTabViewKey))
            {
                ViewModelToViewConverter.DictModelView.Remove(args.CadModelTabViewKey);
            }
        }

        private void OnCadModelLoadedEvent(string modelName)
        {
            FileIsExist = true;
            _numberOfTabPages++;
            //if(++_numberOfTabPages == 1)
            //{
            //    PanCommand_Clicked();
            //}
            ////SetMenuStatesForCurrentTabPage();
            //NavigationMenuCommand_Clicked();
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


            //ProgressNotifier progressNotifier= new();

            //progressNotifier.ProgressMax = 3000;
            //progressNotifier.ProgressStep = 500;

            //_ctsForProgressbar = new();
            //progressNotifier.RunAsync(_ctsForProgressbar, 
            //    _serviceFactory.EventSrv.GetEvent<ProgressStepChangedEvent>,
            //    _serviceFactory.EventSrv.GetEvent<ProgressMaxChangedEvent>);

            _serviceFactory.EventSrv.GetEvent<OpenCadModelTabViewEvent>()
                .Publish(
                    new OpenCadModelTabViewEventArgs(dlg.FileName, Path.GetFileName(dlg.FileName)));
        }
        public void OnMenuExitCommand()
        {
            _serviceFactory.EventSrv.GetEvent<ExitApplicationEvent>().Publish();
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
            //_serviceFactory.EventSrv.GetEvent<PanCommandClickedEvent>().Publish();
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
            ButtonFactory[ButtonName.PanBtn]().IsChecked = true;
            ButtonFactory[ButtonName.OrbitBtn]().IsChecked = false;
            _serviceFactory.EventSrv.GetEvent<OdaMenuCommandClickEvent>()
            .Publish(new OdaMenuCommandClickEventArg() { OdaEventType = OdaEventType.Panning });
        }

        private DelegateCommand _orbitCommand;
        public ICommand OrbitCommand => _orbitCommand ??= new DelegateCommand(OrbitCommand_Clicked).ObservesProperty(() => FileIsExist);

        private void OrbitCommand_Clicked()
        {
            ResetMenuFlags();
            IsNavigation = true;
            ButtonFactory[ButtonName.OrbitBtn]().IsChecked = true;
            ButtonFactory[ButtonName.PanBtn]().IsChecked = false;
            _serviceFactory.EventSrv.GetEvent<OdaMenuCommandClickEvent>()
            .Publish(new OdaMenuCommandClickEventArg() { OdaEventType = OdaEventType.Orbitting });
        }
        private DelegateCommand _aboutCommand;
        public ICommand AboutCommand => _aboutCommand ??= new DelegateCommand(AboutCommand_Clicked).ObservesProperty(() => FileIsExist);

        private void AboutCommand_Clicked()
        {
            //ResetMenuFlags();
            if (_serviceFactory.AppSettings.ShowAboutAnimation)
            {
                var aboutDlg = new AboutTestPad();
                aboutDlg.Show();
            }
        }

        private DelegateCommand<string> _zoomCommand;
        public DelegateCommand<string> ZoomCommand => _zoomCommand ??= new DelegateCommand<string>(param => ZoomCommand_Clicked(param)).ObservesProperty(() => FileIsExist);

        private void ZoomCommand_Clicked(object param)
        {
            switch (param.ToString())
            {
                case "Zoom In":
                    _serviceFactory.EventSrv.GetEvent<OdaMenuCommandClickEvent>()
                    .Publish(new OdaMenuCommandClickEventArg() { OdaEventType = OdaEventType.SetZoom, EZoomType = ZoomType.ZoomIn });
                    break;
                case "Zoom Out":
                    _serviceFactory.EventSrv.GetEvent<OdaMenuCommandClickEvent>()
                    .Publish(new OdaMenuCommandClickEventArg() { OdaEventType = OdaEventType.SetZoom, EZoomType = ZoomType.ZoomOut });
                    break;
                case "Zoom Extents":
                    _serviceFactory.EventSrv.GetEvent<OdaMenuCommandClickEvent>()
                    .Publish(new OdaMenuCommandClickEventArg() { OdaEventType = OdaEventType.SetZoom, EZoomType = ZoomType.ZoomExtents });
                    break;
            }
        }

        // View commands
        private DelegateCommand<string> _viewCommand;
        public DelegateCommand<string> ViewCommand => _viewCommand ??= new DelegateCommand<string>(param => ViewCommand_Clicked(param)).ObservesProperty(() => FileIsExist);

        private void ViewCommand_Clicked(object param)
        {
            switch (param.ToString())
            {
                case "Top":
                    _serviceFactory.EventSrv.GetEvent<OdaMenuCommandClickEvent>()
            .Publish(new OdaMenuCommandClickEventArg() { OdaEventType = OdaEventType.Set3DView, E3DViewType = OdTvExtendedView_e3DViewType.kTop });
                    break;
                case "Bottom":
                    _serviceFactory.EventSrv.GetEvent<OdaMenuCommandClickEvent>()
            .Publish(new OdaMenuCommandClickEventArg() { OdaEventType = OdaEventType.Set3DView, E3DViewType = OdTvExtendedView_e3DViewType.kBottom });
                    break;
                case "Left":
                    _serviceFactory.EventSrv.GetEvent<OdaMenuCommandClickEvent>()
            .Publish(new OdaMenuCommandClickEventArg() { OdaEventType = OdaEventType.Set3DView, E3DViewType = OdTvExtendedView_e3DViewType.kLeft });
                    break;
                case "Right":
                    _serviceFactory.EventSrv.GetEvent<OdaMenuCommandClickEvent>()
            .Publish(new OdaMenuCommandClickEventArg() { OdaEventType = OdaEventType.Set3DView, E3DViewType = OdTvExtendedView_e3DViewType.kRight });
                    break;
                case "Front":
                    _serviceFactory.EventSrv.GetEvent<OdaMenuCommandClickEvent>()
            .Publish(new OdaMenuCommandClickEventArg() { OdaEventType = OdaEventType.Set3DView, E3DViewType = OdTvExtendedView_e3DViewType.kFront });
                    break;
                case "Back":
                    _serviceFactory.EventSrv.GetEvent<OdaMenuCommandClickEvent>()
            .Publish(new OdaMenuCommandClickEventArg() { OdaEventType = OdaEventType.Set3DView, E3DViewType = OdTvExtendedView_e3DViewType.kBack });
                    break;
                case "SW Isometric":
                    _serviceFactory.EventSrv.GetEvent<OdaMenuCommandClickEvent>()
            .Publish(new OdaMenuCommandClickEventArg() { OdaEventType = OdaEventType.Set3DView, E3DViewType = OdTvExtendedView_e3DViewType.kSW });
                    break;
                case "SE Isometric":
                    _serviceFactory.EventSrv.GetEvent<OdaMenuCommandClickEvent>()
            .Publish(new OdaMenuCommandClickEventArg() { OdaEventType = OdaEventType.Set3DView, E3DViewType = OdTvExtendedView_e3DViewType.kSE });
                    break;
                case "NE Isometric":
                    _serviceFactory.EventSrv.GetEvent<OdaMenuCommandClickEvent>()
            .Publish(new OdaMenuCommandClickEventArg() { OdaEventType = OdaEventType.Set3DView, E3DViewType = OdTvExtendedView_e3DViewType.kNE });
                    break;
                case "NW Isometric":
                    _serviceFactory.EventSrv.GetEvent<OdaMenuCommandClickEvent>()
            .Publish(new OdaMenuCommandClickEventArg() { OdaEventType = OdaEventType.Set3DView, E3DViewType = OdTvExtendedView_e3DViewType.kNW });
                    break;
            }
        }

        // Projection commands
        private DelegateCommand<string> _projectionCommand;
        public DelegateCommand<string> ProjectionCommand => _projectionCommand ??= new DelegateCommand<string>(param => ProjectionCommand_Clicked(param)).ObservesProperty(() => FileIsExist);
        private void ProjectionCommand_Clicked(object param)
        {
            switch (param.ToString())
            {
                case "Isometric":
                    _serviceFactory.EventSrv.GetEvent<OdaMenuCommandClickEvent>()
                    .Publish(new OdaMenuCommandClickEventArg()
                    { OdaEventType = OdaEventType.SetProjection, EProjectionType = OdTvGsView_Projection.kParallel });
                    ButtonFactory[ButtonName.IsometricBtn]().IsChecked = true;
                    ButtonFactory[ButtonName.PerspectiveBtn]().IsChecked = false;
                    break;
                case "Perspective":
                    ButtonFactory[ButtonName.PerspectiveBtn]().IsChecked = true;
                    ButtonFactory[ButtonName.IsometricBtn]().IsChecked = false;
                    _serviceFactory.EventSrv.GetEvent<OdaMenuCommandClickEvent>()
                    .Publish(new OdaMenuCommandClickEventArg()
                    { OdaEventType = OdaEventType.SetProjection, EProjectionType = OdTvGsView_Projection.kPerspective });
                    break;
            }
        }

        // Render Mode commands
        private DelegateCommand<string> _renderModeCommand;
        public DelegateCommand<string> RenderModeCommand => _renderModeCommand ??= new DelegateCommand<string>(param => RenderModeCommand_Clicked(param)).ObservesProperty(() => FileIsExist);
        private void RenderModeCommand_Clicked(object param)
        {
            ClearRenderModeButtons();
            switch (param.ToString())
            {
                case "2D Wireframe":
                    ButtonFactory[ButtonName.Wireframe2DBtn]().IsChecked = true;
                    _serviceFactory.EventSrv.GetEvent<OdaMenuCommandClickEvent>()
                    .Publish(new OdaMenuCommandClickEventArg()
                    { OdaEventType = OdaEventType.SetRender, ERenderModeType = OdTvGsView_RenderMode.k2DOptimized });
                    break;
                case "3D Wireframe":
                    ButtonFactory[ButtonName.Wireframe3DBtn]().IsChecked = true;
                    _serviceFactory.EventSrv.GetEvent<OdaMenuCommandClickEvent>()
                    .Publish(new OdaMenuCommandClickEventArg()
                    { OdaEventType = OdaEventType.SetRender, ERenderModeType = OdTvGsView_RenderMode.kWireframe });
                    break;
                case "HiddenLine":
                    ButtonFactory[ButtonName.HiddenLineBtn]().IsChecked = true;
                    _serviceFactory.EventSrv.GetEvent<OdaMenuCommandClickEvent>()
                    .Publish(new OdaMenuCommandClickEventArg()
                    { OdaEventType = OdaEventType.SetRender, ERenderModeType = OdTvGsView_RenderMode.kHiddenLine });
                    break;
                case "Shaded":
                    ButtonFactory[ButtonName.ShadedBtn]().IsChecked = true;
                    _serviceFactory.EventSrv.GetEvent<OdaMenuCommandClickEvent>()
                    .Publish(new OdaMenuCommandClickEventArg()
                    { OdaEventType = OdaEventType.SetRender, ERenderModeType = OdTvGsView_RenderMode.kFlatShaded });
                    break;
                case "Shaded with edges":
                    ButtonFactory[ButtonName.ShadedWithEdgesBtn]().IsChecked = true;
                    _serviceFactory.EventSrv.GetEvent<OdaMenuCommandClickEvent>()
                    .Publish(new OdaMenuCommandClickEventArg()
                    { OdaEventType = OdaEventType.SetRender, ERenderModeType = OdTvGsView_RenderMode.kFlatShadedWithWireframe });
                    break;
            }
        }
        private void ClearRenderModeButtons()
        {
            ButtonFactory[ButtonName.Wireframe2DBtn]().IsChecked = false;
            ButtonFactory[ButtonName.Wireframe3DBtn]().IsChecked = false;
            ButtonFactory[ButtonName.HiddenLineBtn]().IsChecked = false;
            ButtonFactory[ButtonName.ShadedBtn]().IsChecked = false;
            ButtonFactory[ButtonName.ShadedWithEdgesBtn]().IsChecked = false;

        }

        // regen commands
        private DelegateCommand<string> _regenCommand;
        public DelegateCommand<string> RegenCommand => _regenCommand ??= new DelegateCommand<string>(param => RegenCommand_Clicked(param)).ObservesProperty(() => FileIsExist);

        private void RegenCommand_Clicked(object param)
        {
            switch (param.ToString())
            {
                case "RegenAll":
                    _serviceFactory.EventSrv.GetEvent<OdaMenuCommandClickEvent>()
                    .Publish(new OdaMenuCommandClickEventArg()
                    { OdaEventType = OdaEventType.SetRegen, ERegenModeType = OdTvGsDevice_RegenMode.kRegenAll });
                    break;
                case "RegenVisible":
                    _serviceFactory.EventSrv.GetEvent<OdaMenuCommandClickEvent>()
                    .Publish(new OdaMenuCommandClickEventArg()
                    { OdaEventType = OdaEventType.SetRegen, ERegenModeType = OdTvGsDevice_RegenMode.kRegenVisible });
                    break;
                case "RegenView":
                    _serviceFactory.EventSrv.GetEvent<OdaMenuCommandClickEvent>()
                    .Publish(new OdaMenuCommandClickEventArg()
                    { OdaEventType = OdaEventType.RegenView });
                    break;
                    }
        }
    }
}

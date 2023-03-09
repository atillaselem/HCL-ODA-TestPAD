using HCL_ODA_TestPAD.Mvvm;
using HCL_ODA_TestPAD.Mvvm.Events;
using HCL_ODA_TestPAD.Services;
using Prism.Commands;
using Prism.Events;
using System.Threading.Tasks;
using System.Windows.Input;
using Teigha.Visualize;

namespace HCL_ODA_TestPAD.ViewModels.Base
{
    public abstract class CadImageTabViewModelBase : BindableBase, ICadImageTabViewModel
    {
        protected readonly IEventAggregator EventAggregator;
        protected readonly IMessageDialogService MessageDialogService;
        protected readonly IConsoleService ConsoleService;
        public string CadImageFilePath { get; set; }
        public ICadOdaMenuView CadOdaMenuView { get; set; }
        public ICommand CloseCadImageTabViewCommand { get; }
        public string TabItemTitle { get; set; }

        public CadImageTabViewModelBase(IEventAggregator eventAggregator,
          IMessageDialogService messageDialogService,
          IConsoleService consoleService)
        {
            EventAggregator = eventAggregator;
            MessageDialogService = messageDialogService;
            ConsoleService = consoleService;
            CloseCadImageTabViewCommand = new DelegateCommand(OnCloseCadImageTabViewExecute);
        }


        public abstract Task LoadCadModelViewAsync();
        public abstract void CloseTabView();

        protected virtual void OnCloseCadImageTabViewExecute()
        {
            CloseTabView();
            EventAggregator.GetEvent<CloseCadModelTabViewEvent>()
              .Publish(new CloseCadModelTabViewEventArgs
              {
                  CadModelTabViewKey = TabItemTitle
              });
        }

        public void OnPanClicked()
        {
            CadOdaMenuView?.Pan();
        }
        public void OnOrbitClicked()
        {
            CadOdaMenuView?.Orbit();
        }
        public void OnZoomClicked(ZoomType zoomType)
        {
            CadOdaMenuView?.SetZoom(zoomType);
        }
        public void OnSet3DViewClicked(OdTvExtendedView.e3DViewType e3DViewType)
        {
            CadOdaMenuView?.Set3DView(e3DViewType);
        }
        public void OnRenderModeClicked(OdTvGsView.RenderMode eRenderModeType)
        {
            CadOdaMenuView?.SetRenderMode(eRenderModeType);
        }
        public void OnSetProjectionClicked(OdTvGsView.Projection eProjectionType)
        {
            CadOdaMenuView?.SetProjectionType(eProjectionType);
        }
        public void OnRegenModeClicked(OdTvGsDevice.RegenMode eRegenModeType)
        {
            CadOdaMenuView?.Regen(eRegenModeType);
        }
        public void OnRegenViewClicked()
        {
            CadOdaMenuView?.RegenView();
        }

    }

}

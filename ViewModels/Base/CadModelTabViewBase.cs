using HCL_ODA_TestPAD.Mvvm;
using HCL_ODA_TestPAD.Mvvm.Events;
using HCL_ODA_TestPAD.Services;
using HCL_ODA_TestPAD.Views;
using Prism.Commands;
using Prism.Events;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

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

        public void OnZoomInClicked()
        {
            //CadOdaMenuView?.
        }

        public void OnZoomOutClicked()
        {
            throw new NotImplementedException();
        }

        public void OnZoomExtentClicked()
        {
            throw new NotImplementedException();
        }
    }

}

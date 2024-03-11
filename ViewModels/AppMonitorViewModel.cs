using HCL_ODA_TestPAD.Mvvm;
using HCL_ODA_TestPAD.Mvvm.Events;
using HCL_ODA_TestPAD.Settings;
using HCL_ODA_TestPAD.Utility;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace HCL_ODA_TestPAD.ViewModels
{
    public class AppMonitorViewModel : BindableBase
    {
        private readonly IServiceFactory _serviceFactory;
        private readonly ObservableCollection<ConsoleTabItemViewModel> _consoleTabViewItems;
        private string _appConsoleText;
        private ConsoleTabItemViewModel _activeTabItem;

        /// <summary>
        ///     List that the TabControl's ItemsSource property is bound to
        /// </summary>
        public ObservableCollection<ConsoleTabItemViewModel> ConsoleTabViewItems => _consoleTabViewItems;

        public string AppConsoleText
        {
            get => _appConsoleText;
            set => SetProperty(ref _appConsoleText, value);
        }

        private bool _isBorderVisible;
        public bool IsBorderVisible
        {
            get => _isBorderVisible;
            set => SetProperty(ref _isBorderVisible, value);
        }

        public AppMonitorViewModel(IServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
            _consoleTabViewItems = new ObservableCollection<ConsoleTabItemViewModel>();
            _isBorderVisible = true;
            SubscribeEvents();
            SetConsoleOutput();
        }

        private void SubscribeEvents()
        {
            _serviceFactory.EventSrv.GetEvent<CadModelLoadedEvent>().Subscribe(OnCadModelLoadedEvent);
            _serviceFactory.EventSrv.GetEvent<CloseCadModelTabViewEvent>().Subscribe(OnCloseCadModelTabViewEvent);
            //_eventAggregator.GetEvent<TabPageSelectionChangedEvent>().Subscribe(OnTabPageSelectionChangedEvent);
        }

        private void SetConsoleOutput()
        {
            using (var consoleWriter = new ConsoleWriter())
            {
                consoleWriter.WriteLineEvent += OnConsoleWriteLineEvent;

                Console.SetOut(consoleWriter);
            }

            SetWelcomeText();
        }

        private void OnConsoleWriteLineEvent(object sender, ConsoleWriterEventArgs e)
        {
            //AppConsoleText += $"{DateTime.Now:[HH-mm-ss]} : " + e.Value + Environment.NewLine;
            var lineText = $"{DateTime.Now:[HH-mm-ss]} : " + e.Value + Environment.NewLine;
            ForwardLineTextToActiveTab(lineText);
        }

        private void ForwardLineTextToActiveTab(string lineText)
        {
            if (_activeTabItem != null && _activeTabItem.IsTabItemRunning)
            {
                _activeTabItem.TabItemContent += lineText;
            }
            else
            {
                AppConsoleText += lineText;
            }
        }

        private void OnConsoleWriteEvent(object sender, ConsoleWriterEventArgs e)
        {
            ForwardLineTextToActiveTab(e.Value);
        }

        private void OnCadModelLoadedEvent(string filePath)
        {
            var timeStamp = DateTime.Now.ToString("[HH-mm-ss] : ");
            var newTabItem = new ConsoleTabItemViewModel()
            {
                TabItemHeaderText = Path.GetFileName(filePath),
                TabItemContent = $"{timeStamp}{filePath} loaded." + Environment.NewLine,
                IsTabItemRunning = true
            };
            ConsoleTabViewItems.Add(newTabItem);
            _activeTabItem = newTabItem;
            IsBorderVisible = false;
        }
        private void OnCloseCadModelTabViewEvent(CloseCadModelTabViewEventArgs args)
        {
            var tabItem = ConsoleTabViewItems.SingleOrDefault(item => item.TabItemHeaderText == args.CadModelTabViewKey);
            if (tabItem != null)
                ConsoleTabViewItems.Remove(tabItem);
        }

        private void OnTabPageSelectionChangedEvent(int selectedIndex)
        {
            ConsoleTabViewItems.RemoveAt(selectedIndex);
        }
        private void SetWelcomeText()
        {
            AppConsoleText = string.Concat(Enumerable.Repeat("=", 50)) + Environment.NewLine;
            AppConsoleText += $"{DateTime.Now:[HH-mm-ss]} : Welcome to {AssemblyHelper.GetAppTitle()}" + Environment.NewLine;
            AppConsoleText += string.Concat(Enumerable.Repeat("=", 50)) + Environment.NewLine + Environment.NewLine;
        }

        //private void OnEvent_Logging(AppEvent<EMIT, object> obj)
        //{
        //    var timeStamp = obj.When.ToString("[HH-mm-ss] : ");
        //    var msg = obj.Content.ToString();
        //    string sender = String.Empty;
        //    if ((obj.Message == EMIT.ERROR || obj.Message == EMIT.FATAL || obj.Message == EMIT.WARNING) && obj.CallerInfo != null)
        //    {
        //        sender = $" - CallerInfo : Sender [{obj.Who ?? "null"}] Member Method [{obj.CallerInfo.MemberName}]";
        //    }
        //    var newLine = $"{timeStamp}{msg}{sender}";
        //    lock (_lock)
        //    {
        //        if (_activeTabItem != null && _activeTabItem.IsTabItemRunning)
        //        {
        //            _activeTabItem.TabItemContent += newLine + Environment.NewLine;
        //        }
        //        else
        //        {
        //            AppConsoleText += newLine + Environment.NewLine;
        //        }
        //    }
        //}

        ///// <summary>
        ///// Unsubscribe from application events.
        ///// </summary>
        //public void UnSubscribeAppEvents()
        //{
        //    //FxBroker<EMIT>.Instance.UnSubscribe(EMIT.PRISM, OnEvent_Emit);
        //}
    }
}

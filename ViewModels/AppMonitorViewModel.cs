using HCL_ODA_TestPAD.Mvvm;
using HCL_ODA_TestPAD.Utility;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace HCL_ODA_TestPAD.ViewModels
{
    public class AppMonitorViewModel : BindableBase
    {
        private readonly ObservableCollection<ConsoleTabItemViewModel> _consoleTabViewItems;
        private string _appConsoleText;
        private ConsoleTabItemViewModel _activeTabItem;
        private object _lock = new object();

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

        public AppMonitorViewModel()
        {
            _consoleTabViewItems = new ObservableCollection<ConsoleTabItemViewModel>();
            _isBorderVisible = true;
            SubscribeEvents();
            SetConsoleOutput();
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
            //AppConsoleText += e.Value;
            ForwardLineTextToActiveTab(e.Value);
        }

        /// <summary>
        /// Subscribes the application events.
        /// </summary>
        public void SubscribeEvents()
        {
            //foreach (var emit in EnumUtil.Enumerate<EMIT>().Where(e => e <= EMIT.FATAL))
            //{
            //    FxBroker<EMIT>.Instance.Subscribe(emit, OnEvent_Logging);
            //}
            //FxBroker<CREATE>.Instance.Subscribe(CREATE.EVENT_NEW_TESTCASE, OnEvent_NewTestCase);
            //FxBroker<AET>.Instance.Subscribe(AET.EVENT_USE_CASE_IDENTITY, OnEvent_UseCaseIdentity);
        }

        //private void OnEvent_UseCaseIdentity(AppEvent<AET, object> obj)
        //{
        //    var idBuilder = new StringBuilder();
        //    var timeStamp = obj.When.ToString("[HH-mm-ss] : ");
        //    var ucIdentity = (UseCaseIdentity)obj.Content;
        //    idBuilder.AppendLine($"{timeStamp}Completed Successfully.");
        //    idBuilder.AppendLine("--------------------------------------------");
        //    idBuilder.AppendLine($"Use Case Name : {ucIdentity.UseCaseName}");
        //    idBuilder.AppendLine($"Category : {ucIdentity.UseCaseCategory}");
        //    idBuilder.AppendLine($"Log File : {ucIdentity.LogFile}");
        //    idBuilder.AppendLine("--------------------------------------------");
        //    if (_activeTabItem != null)
        //    {
        //        _activeTabItem.TabItemContent += idBuilder;
        //        _activeTabItem.IsTabItemRunning = false;
        //    }
        //}

        //private void OnEvent_NewTestCase(AppEvent<CREATE, object> obj)
        //{
        //    var timeStamp = obj.When.ToString("[HH-mm-ss] : ");
        //    var newTabItem = new TestConsoleTabItemViewModel()
        //    {
        //        TabItemHeaderText = $"[{ConsoleTabViewItems.Count + 1}]-{obj.Content}",
        //        TabItemContent = $"{timeStamp}{obj.Content} started.." + Environment.NewLine,
        //        IsTabItemRunning = true
        //    };
        //    ConsoleTabViewItems.Add(newTabItem);
        //    _activeTabItem = newTabItem;
        //    IsBorderVisible = false;
        //    //SetWelcomeText();
        //}

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

using HCL_ODA_TestPAD.Mvvm;

namespace HCL_ODA_TestPAD.ViewModels
{
    public class ConsoleTabItemViewModel : BindableBase
    {
        private string _tabItemHeaderText;
        public string TabItemHeaderText
        {
            get => _tabItemHeaderText;
            set => SetProperty(ref _tabItemHeaderText, value);
        }

        private string _tabItemContent;
        public string TabItemContent
        {
            get => _tabItemContent;
            set => SetProperty(ref _tabItemContent, value);
        }

        private bool _isTabItemRunning;
        public bool IsTabItemRunning
        {
            get => _isTabItemRunning;
            set => SetProperty(ref _isTabItemRunning, value);
        }

    }
}



using HCL_ODA_TestPAD.Mvvm;
using System.Threading;

namespace HCL_ODA_TestPAD.ViewModels
{
    public class OverlayViewModel : BindableBase
    {
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }


        private string _title;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public CancellationTokenSource CancelTokenSource { get; set; }
        public CancellationToken Token => CancelTokenSource.Token;
    }
}

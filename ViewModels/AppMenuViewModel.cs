using HCL_ODA_TestPAD.Mvvm;
using Prism.Events;

namespace HCL_ODA_TestPAD.ViewModels
{
    public class AppMenuViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;

        public AppMenuViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

    }
}

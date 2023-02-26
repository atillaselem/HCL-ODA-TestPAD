using Prism.Events;

namespace HCL_ODA_TestPAD.Mvvm.Events
{
    public class CloseCadModelTabViewEvent : 
        PubSubEvent<CloseCadModelTabViewEventArgs>
    {
    }
    public class CloseCadModelTabViewEventArgs
    {
        public string CadModelTabViewKey { get; set; }
    }
}

using Prism.Events;

namespace HCL_ODA_TestPAD.Mvvm.Events
{
    public class CloseCadModelTabViewEvent : 
        PubSubEvent<CloseCadModelTabViewEventArgs>
    {
    }

    public record struct CloseCadModelTabViewEventArgs(string CadModelTabViewKey);

}

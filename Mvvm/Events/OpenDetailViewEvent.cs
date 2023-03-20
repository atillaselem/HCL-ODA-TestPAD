using Prism.Events;

namespace HCL_ODA_TestPAD.Mvvm.Events
{
    public class OpenCadModelTabViewEvent : PubSubEvent<OpenCadModelTabViewEventArgs>
    {

    }

    public record struct OpenCadModelTabViewEventArgs(string ViewModelFilePath, string ViewModelKey);
}

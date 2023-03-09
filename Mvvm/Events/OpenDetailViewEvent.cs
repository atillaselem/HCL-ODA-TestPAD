using Prism.Events;

namespace HCL_ODA_TestPAD.Mvvm.Events
{
    public class OpenCadModelTabViewEvent : PubSubEvent<OpenCadModelTabViewEventArgs>
    {

    }
    public class OpenCadModelTabViewEventArgs
    {
        public string ViewModelFilePath { get; set; }
        public string ViewModelKey { get; set; } 
    }
}

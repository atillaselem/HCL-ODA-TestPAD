using Prism.Events;

namespace HCL_ODA_TestPAD.Mvvm.Events
{
    public class AppStatusTextChanged : PubSubEvent<string> { }
    public class ScreenCoordinatesChanged : PubSubEvent<string> { }
    public class ScaleFactorChanged : PubSubEvent<string> { }
}

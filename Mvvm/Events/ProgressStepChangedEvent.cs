using HCL_ODA_TestPAD.ViewModels.Base;
using Prism.Events;
using Teigha.Visualize;

namespace HCL_ODA_TestPAD.Mvvm.Events
{
    public class ProgressStepChangedEvent : PubSubEvent<ProgressStepChangedEventArg> { }
    public class ProgressStepChangedEventArg
    {
        public int CurrentProgressStep { get; set; }
        public double RegenThreshold { get; set; }
        public double CurrentDeviceCoefficient { get; set; }
        public double LastDeviceCoefficientAfterRegen { get; set; }
    }
}

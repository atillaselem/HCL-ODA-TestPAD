using Prism.Events;

namespace HCL_ODA_TestPAD.Mvvm.Events
{
    public class ProgressStepChangedEvent : PubSubEvent<ProgressStepChangedEventArg> { }
    public record struct ProgressStepChangedEventArg
    {
        public int CurrentProgressStep { get; init; }
        public double RegenThreshold { get; init; }
        public double CurrentDeviceCoefficient { get; init; }
        public double LastDeviceCoefficientAfterRegen { get; init; }
    }
}

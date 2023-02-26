using System;

namespace HCL_ODA_TestPAD.EventBroker
{
    /// <summary>
    /// Application wide Factory class to use Event Broker.
    /// </summary>
    //public static class Fx_Broker
    //{
    //    /// <summary>
    //    /// The Event Broker Instance
    //    /// </summary>
    //    public static readonly IEventBroker<AET, object> Instance = EventBroker<AET, object>.Instance;

    //}

    public class FxBroker<T> where T : struct, IConvertible
    {
        private FxBroker(T enumEventType)
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("Fx_Broker Exception : Generic Type is not an enum!");
        }

        private static readonly Lazy<IEventBroker<T, object>> instance = new Lazy<IEventBroker<T, object>>(() => EventBroker<T, object>.Instance);

        public static IEventBroker<T, object> Instance => instance.Value;
    }
}
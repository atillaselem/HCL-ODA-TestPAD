namespace HCL_ODA_TestPAD.EventBroker
{
    public interface IAppEventListener
    {
        void SubscribeAppEvents();

        void UnSubscribeAppEvents();
    }
}
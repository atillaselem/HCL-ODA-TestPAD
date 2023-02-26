using System;

namespace HCL_ODA_TestPAD.EventBroker
{
    /// <summary>
    /// Event Message Info Type
    /// </summary>
    public enum EMIT
    {
        DEBUG,
        WARNING,
        INFO,
        ERROR,
        FATAL,
        APP_EXCEPTION,
        TEST,
        USER,
        PRISM,
        APP,
        PASS,
        FAIL,
        WPF_EXCEPTION,
        MODULE_LOADED,
        MODULE_FAILED,
        MODULES_INITIALIZED
    }
    public interface IEventMessageInfo
    {
        DateTime EventTime { get; set; }
        EMIT EventType { get; set; }
        string EventName { get; set; }
        Type EventSender { get; set; }
        Type EventReceiver { get; set; }
        string EventMessage { get; set; }
        Exception OriginalException { get; set; }
        Exception InnerException { get; set; }
        int EventId { get; set; }
        ICallerInfo CallerInfo { get; set; }
    }
    public class EventMessageInfo : IEventMessageInfo
    {
        public DateTime EventTime { get; set; }
        public EMIT EventType { get; set; }
        public string EventName { get; set; }
        public Type EventSender { get; set; }
        public Type EventReceiver { get; set; }
        public string EventMessage { get; set; }
        public Exception OriginalException { get; set; }
        public Exception InnerException { get; set; }
        public int EventId { get; set; }
        public ICallerInfo CallerInfo { get; set; }
    }
}
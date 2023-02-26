using System;

namespace HCL_ODA_TestPAD.EventBroker
{
    public interface ICallerInfo
    {
        string FilePath { get; set; }
        int LineNumber { get; set; }
        string MemberName { get; set; }
    }
    public class CallerInfo : ICallerInfo
    {
        public string FilePath { get; set; }
        public int LineNumber { get; set; }
        public string MemberName { get; set; }
    }

    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="T">Typed instance of AppEvent Message Type</typeparam>
    /// <typeparam name="D">Typed instance of Content of AppEvent Message Type</typeparam>
    public class AppEvent<T, D>
    {
        /// <summary>
        /// Instance of class who publish/subscribe to event.
        /// </summary>
        public object Who { get; private set; }

        /// <summary>
        /// Typed instance of AppEvent Message Type
        /// </summary>
        public T Message { get; private set; }

        /// <summary>
        /// Typed instance of Content of AppEvent Message Type
        /// </summary>
        public D Content { get; private set; }

        /// <summary>
        /// DateTime of AppEvent
        /// </summary>
        public DateTime When { get; private set; }

        public CallerInfo CallerInfo { get; private set; }

        /// <summary>
        /// Constructs an AppEvent instance.
        /// </summary>
        /// <param name="source">Source of AppEvent</param>
        /// <param name="message">Message of AppEvent published/subscribed</param>
        /// <param name="content">Content of Message published.</param>
        public AppEvent(object source, T message, D content)
        {
            Who = source;
            Content = content;
            When = DateTime.UtcNow;
            Message = message;
        }

        public AppEvent(object source, T message, D content, CallerInfo ci)
        {
            Who = source;
            Content = content;
            When = DateTime.UtcNow;
            Message = message;
            CallerInfo = ci;
        }
    }
}
using System;
using System.Runtime.CompilerServices;

namespace HCL_ODA_TestPAD.EventBroker
{
    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="D"></typeparam>
    public interface IEventBroker<T, D>
    {
        /// <summary>
        /// Publishing an AppEvent Typed instance.
        /// </summary>
        /// <param name="source">Source of AppEvent</param>
        /// <param name="message">Message of AppEvent published</param>
        /// <param name="payload">The payload.</param>
        void Publish(object source, T message, D payload);

        /// <summary>
        /// Publishing an AppEvent Typed instance.
        /// </summary>
        /// <param name="source">Source of AppEvent</param>
        /// <param name="message">Message of AppEvent published</param>
        /// <param name="payload">The payload.</param>
        /// <param name="ci">The caller info</param>
        void PublishEx(object source, T message, D payload,
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerMemberName] string callerMember = "");

        /// <summary>
        /// Async subscription of AppEvent Typed instance.
        /// </summary>
        /// <param name="source">Source of AppEvent</param>
        /// <param name="message">Message of AppEvent subscribed</param>
        /// <param name="payload">The payload.</param>
        void PublishAsync(object source, T message, D payload);

        /// <summary>
        /// Sync subscription of AppEvent Typed instance.
        /// </summary>
        /// <param name="message">Message of AppEvent subscribed</param>
        /// <param name="subscription">The subscription.</param>
        void Subscribe(T message, Action<AppEvent<T, D>> subscription);

        /// <summary>
        /// Sync unsubscription of AppEvent Typed instance.
        /// </summary>
        /// <param name="message">Message of AppEvent Type to be unsubscribed</param>
        /// <param name="subscription">List of Actions already subscribed for AppEvent Type</param>
        void UnSubscribe(T message, Action<AppEvent<T, D>> subscription);
    }
}
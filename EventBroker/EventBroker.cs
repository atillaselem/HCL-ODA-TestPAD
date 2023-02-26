using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace HCL_ODA_TestPAD.EventBroker
{
    /// <summary>
    /// An Event Broker Implementation including PublishAsync with Task Parallel Library.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="D"></typeparam>
    public class EventBroker<T, D> : IEventBroker<T, D>, IDisposable
    {
        #region Private Instance Variables

        private readonly Dictionary<T, List<Action<AppEvent<T, D>>>> _subscribers;

        #endregion Private Instance Variables

        #region Private Instance Methods

        // Collect all Private Instance Methods

        #endregion Private Instance Methods

        #region Constructors & Finalizers

        /// <summary>
        /// Prevents a default instance of the <see cref="EventBroker{T, D}"/> class from being created.
        /// </summary>
        private EventBroker()
        {
            _subscribers = new Dictionary<T, List<Action<AppEvent<T, D>>>>();
        }

        #endregion Constructors & Finalizers

        #region Public Instance Properties

        // Collect all Public Instance Properties

        #endregion Public Instance Properties

        #region Public Instance Methods

        /// <summary>
        /// Publishing an AppEvent Typed instance.
        /// </summary>
        /// <param name="source">Source of AppEvent</param>
        /// <param name="message">Message of AppEvent published</param>
        /// <param name="payload"></param>
        public void Publish(object source, T message, D payload)
        {
            if (message == null)
                return;

            if (_subscribers.TryGetValue(message, out var delegates))
            {
                var data = new AppEvent<T, D>(source, message, payload);
                foreach (var handler in delegates.Select
                (item => item))
                {
                    handler?.Invoke(data);
                }
            }
        }

        /// <summary>
        /// Publishes an AppEvent Typed instance with Caller Information.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="message">The message.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="callerFilePath">The caller file path.</param>
        /// <param name="callerLineNumber">The caller line number.</param>
        /// <param name="callerMember">The caller member.</param>
        public void PublishEx(object source, T message, D payload,
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerMemberName] string callerMember = "")
        {
            if (message == null)
                return;

            if (_subscribers.TryGetValue(message, out var delegates))
            {
                var ci = new CallerInfo() { FilePath = callerFilePath, LineNumber = callerLineNumber, MemberName = callerMember };
                var data = new AppEvent<T, D>(source, message, payload, ci);
                foreach (var handler in delegates.Select
                (item => item))
                {
                    handler?.Invoke(data);
                }
            }
        }

        /// <summary>
        /// Async subscription of AppEvent Typed instance.
        /// </summary>
        /// <param name="source">Source of AppEvent</param>
        /// <param name="message">Message of AppEvent subscribed</param>
        /// <param name="payload"></param>
        public void PublishAsync(object source, T message, D payload)
        {
            if (message == null)
                return;

            if (_subscribers.TryGetValue(message, out var delegates))
            {
                var data = new AppEvent<T, D>(source, message, payload);
                foreach (var handler in delegates.Select
                (item => item))
                {
                    Task.Factory.StartNew(() => handler?.Invoke(data));
                }
            }
        }

        /// <summary>
        /// Sync subscription of AppEvent Typed instance.
        /// </summary>
        /// <param name="message">Message of AppEvent subscribed</param>
        /// <param name="subscription"></param>
        public void Subscribe(T message, Action<AppEvent<T, D>> subscription)
        {
            if (!_subscribers.TryGetValue(message, out var delegates))
            {
                delegates = new List<Action<AppEvent<T, D>>>();
                _subscribers[message] = delegates;
            }
            delegates.Add(subscription);
        }

        /// <summary>
        /// Sync unsubscription of AppEvent Typed instance.
        /// </summary>
        /// <param name="message">Message of AppEvent Type to be unsubscribed</param>
        /// <param name="subscription">List of Actions already subscribed for AppEvent Type</param>
        public void UnSubscribe(T message, Action<AppEvent<T, D>> subscription)
        {
            if (_subscribers.TryGetValue(message, out var delegates))
            {
                delegates.Remove(subscription);
                if (delegates.Count == 0)
                {
                    _subscribers.Remove(message);
                }
            }
        }

        #endregion Public Instance Methods

        #region Private Static Variables

        /// <summary>
        /// Thread-safe instance of Event Broker.
        /// </summary>
        private static readonly EventBroker<T, D> _instance = new EventBroker<T, D>();

        #endregion Private Static Variables

        #region Public Static Properties

        /// <summary>
        /// Gets the Event Broker instance.
        /// </summary>
        /// <value>
        /// The singleton instance.
        /// </value>
        public static EventBroker<T, D> Instance => _instance;

        #endregion Public Static Properties

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        ///
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _subscribers?.Clear();
                }
                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        /// <summary>
        ///
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }

        #endregion IDisposable Support
    }
}
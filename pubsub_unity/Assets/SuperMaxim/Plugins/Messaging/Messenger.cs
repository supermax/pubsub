using System;
using System.Collections.Generic;
using SuperMaxim.Core.Extensions;
using SuperMaxim.Core.Objects;
using System.Threading;
using SuperMaxim.Core.Logging;
using SuperMaxim.Core.Threading;
using SuperMaxim.Messaging.API;
using SuperMaxim.Messaging.Monitor;

namespace SuperMaxim.Messaging
{
    /// <summary>
    /// Pub/Sub Messenger Singleton
    /// <remarks>(Implements <see cref="IMessenger"/>)</remarks>
    /// </summary>
    public sealed class Messenger : Singleton<IMessenger, Messenger>, IMessenger
    {
        // Mapping [PAYLOAD_TYPE]->[MAP(INT, SUBSCRIBER)]
        private readonly Dictionary<Type, SubscriberSet> _subscribersSet = new();

        // List of subscribers to optimize iteration during subscribers processing
        private readonly List<Subscriber> _subscribers = new();

        // List of subscribers to optimize add (subscribe) operation
        private readonly List<Subscriber> _add = new();

        private readonly ILogger _logger = Loggers.Console;

        /// <summary>
        /// Static CTOR to initialize other monitoring tools (singletons)
        /// </summary>
        static Messenger()
        {
            // init MainThreadDispatcher and print main thread ID
            Loggers.Console.LogInfo("Main Thread ID: {0}", UnityMainThreadDispatcher.Default.ThreadId);

#if DEBUG
            // init MessengerMonitor
            Loggers.Console.LogInfo("Messenger Monitor {0}", MessengerMonitor.Default);
#endif
        }

        /// <summary>
        /// Publish payload (inner method)
        /// </summary>
        /// <param name="payloadType">The type of the payload</param>
        /// <param name="payload">The payload</param>
        /// <exception cref="ArgumentNullException">Exception is thrown in case <param name="payloadType"></param> is null</exception>
        /// <exception cref="ArgumentNullException">Exception is thrown in case <param name="payload"></param> is null</exception>
        /// <exception cref="InvalidCastException">Exception is thrown in case <param name="payload"></param> type doesn't match <param name="payloadType"></param></exception>
        /// <returns>Instance of the Messenger</returns>
        private void PublishInternal(Type payloadType, object payload)
        {
            if(payloadType == null)
            {
                throw new ArgumentNullException(nameof(payloadType));
            }
            if(payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }
            if (payload.GetType() != payloadType)
            {
                throw new InvalidCastException($"{nameof(payload)} type `{payload}` doesn't match to {payloadType}");
            }
            // exit method, if subscribers' dic. does not contain the given payload type
            if (!_subscribersSet.ContainsKey(payloadType))
            {
                return;
            }

            try
            {
                // get subscriber's dic. for the payload type
                _subscribersSet.TryGetValue(payloadType, out var callbacks);
                // check if "callbacks" dic. is null or empty
                if (callbacks.IsNullOrEmpty())
                {
                    // remove payload type key is "callbacks" dic is empty
                    _subscribersSet.Remove(payloadType);
                    return;
                }

                callbacks?.Publish(payload);
            }
            finally
            {
                // process pending tasks
                Process();
            }
        }

        /// <summary>
        /// Publish given payload to relevant subscribers
        /// </summary>
        /// <param name="payload">Instance of payload to publish</param>
        /// <typeparam name="T">The type of payload to publish</typeparam>
        /// <exception cref="ArgumentNullException">Exception is thrown in case <param name="payload"></param> is null</exception>
        /// <returns>Instance of the Messenger</returns>
        public IMessengerPublish Publish<T>(T payload) where T : class, new()
        {
            if(payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            // if calling thread is same as main thread, call underlying method directly
            if(Thread.CurrentThread.ManagedThreadId == UnityMainThreadDispatcher.Default.ThreadId)
            {
                PublishInternal(typeof(T), payload);
                return this;
            }

            // add "act" into "MainThreadDispatcher" queue
            UnityMainThreadDispatcher.Default.Dispatch((Action<Type, object>)PublishInternal, new object[] { typeof(T), payload });
            return this;
        }

         /// <summary>
        /// Publish payload
        /// </summary>
        /// <param name="payloadType">The type of the payload</param>
        /// <param name="payload">The payload</param>
        /// <exception cref="ArgumentNullException">Exception is thrown in case <param name="payloadType"></param> is null</exception>
        /// <exception cref="ArgumentNullException">Exception is thrown in case <param name="payload"></param> is null</exception>
        /// <exception cref="InvalidCastException">Exception is thrown in case <param name="payload"></param> type doesn't match <param name="payloadType"></param></exception>
        /// <returns>Instance of the Messenger</returns>
        public IMessengerPublish Publish(Type payloadType, object payload)
        {
            if(payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            // if calling thread is same as main thread, call underlying method directly
            if(Thread.CurrentThread.ManagedThreadId == UnityMainThreadDispatcher.Default.ThreadId)
            {
                PublishInternal(payloadType, payload);
                return this;
            }

            // add "act" into "MainThreadDispatcher" queue
            UnityMainThreadDispatcher.Default.Dispatch((Action<Type, object>)PublishInternal, new [] { payloadType, payload });
            return this;
        }

        /// <summary>
        /// Subscribe the callback to specified payload type <see cref="T"/>
        /// </summary>
        /// <param name="callback">Callback delegate</param>
        /// <param name="predicate">Callback's predicate</param>
        /// <typeparam name="T">The type of the payload</typeparam>
        /// <exception cref="ArgumentNullException">Exception is thrown in case <param name="callback"></param> is null</exception>
        /// <returns>Messenger instance</returns>
        public IMessengerSubscribe Subscribe<T>(Action<T> callback, Predicate<T> predicate = null) where T : class, new()
        {
            if(callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            // execute subscribe method on main thread
            SubscribeInternal(callback, predicate);
            return this;
        }

        /// <summary>
        /// Subscribe given callback to receive payload with state object
        /// </summary>
        /// <param name="callback">The callback that will receive the payload</param>
        /// <param name="predicate">Callback's predicate</param>
        /// <param name="stateObj">The state object</param>
        /// <typeparam name="TC">The type of payload to receive for the given callback</typeparam>
        /// <typeparam name="TS">The type of state object to receive for the given callback</typeparam>
        public IMessengerSubscribe Subscribe<TC, TS>(
            Action<TC, TS> callback
            , Func<TC, TS, bool> predicate = null
            , TS stateObj = default)
            where TC : class, new()
        {
            // capture the type of the payload
            var key = typeof(TC);
            // init new subscriber instance
            var sub = new Subscriber(key, callback, predicate, _logger, stateObj);

            // check if messenger is busy with publishing payloads
            if (_subscribersSet.ContainsKey(key) && _subscribersSet[key].IsPublishing)
            {
                // add subscriber into "Add" queue if messenger is busy with publishing
                _add.Add(sub);
                return this;
            }

            // if messenger is not busy with publishing, add into subscribers list
            SubscribeInternal(sub);
            return this;
        }

        /// <summary>
        /// Subscribe predicate to filter irrelevant payloads per given type <typeparam name="T"/>
        /// </summary>
        /// <param name="predicate">The predicate to filter irrelevant payloads per given type</param>
        /// <typeparam name="T">The type of payload to receive</typeparam>
        /// <exception cref="ArgumentNullException">Exception is thrown in case <param name="predicate"></param> is null</exception>
        /// <returns>Instance of the Messenger</returns>
        public IMessengerSubscribe Subscribe<T>(Predicate<T> predicate) where T : class, new()
        {
            if(predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            // capture the type of the payload
            var key = typeof(T);
            // init new subscriber instance
            var sub = new Subscriber(key, predicate, _logger, null);

            // check if messenger is busy with publishing payloads
            if (_subscribersSet.ContainsKey(key) && _subscribersSet[key].IsPublishing)
            {
                // add subscriber into "Add" queue if messenger is busy with publishing
                _add.Add(sub);
                return this;
            }
            // if messenger is not busy with publishing, add into subscribers list
            SubscribeInternal(sub);
            return this;
        }

        /// <summary>
        /// Subscribe predicate to filter irrelevant payloads per given type <typeparam name="TC"/>
        /// </summary>
        /// <param name="predicate">The predicate to filter irrelevant payloads</param>
        /// <param name="stateObj">The state object</param>
        /// <typeparam name="TC">The type of payload to receive</typeparam>
        /// <typeparam name="TS">The type of state object to receive for the given callback</typeparam>
        /// <returns>Instance of the Messenger</returns>
        public IMessengerSubscribe Subscribe<TC, TS>(Func<TC, TS, bool> predicate, TS stateObj = default) where TC : class, new()
        {
            if(predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            // capture the type of the payload
            var key = typeof(TC);
            // init new subscriber instance
            var sub = new Subscriber(key, predicate, _logger, stateObj);

            // check if messenger is busy with publishing payloads
            if (_subscribersSet.ContainsKey(key) && _subscribersSet[key].IsPublishing)
            {
                // add subscriber into "Add" queue if messenger is busy with publishing
                _add.Add(sub);
                return this;
            }
            // if messenger is not busy with publishing, add into subscribers list
            SubscribeInternal(sub);
            return this;
        }

        /// <summary>
        /// Subscribe the callback to specified payload type <see cref="T"/>
        /// </summary>
        /// <remarks>
        /// Used internally by messenger to sync threads
        /// </remarks>
        /// <param name="callback">Callback delegate</param>
        /// <param name="predicate">Predicate delegate (optional)</param>
        /// <typeparam name="T">The type of the payload</typeparam>
        private void SubscribeInternal<T>(Action<T> callback, Predicate<T> predicate = null)
        {
            // capture the type of the payload
            var key = typeof(T);
            // init new subscriber instance
            var sub = new Subscriber(key, callback, predicate, _logger, null);

            // check if messenger is busy with publishing payloads
            if (_subscribersSet.ContainsKey(key) && _subscribersSet[key].IsPublishing)
            {
                // add subscriber into "Add" queue if messenger is busy with publishing
                _add.Add(sub);
                return;
            }
            // if messenger is not busy with publishing, add into subscribers list
            SubscribeInternal(sub);
        }

        /// <summary>
        /// Adds subscriber into subscribers list
        /// </summary>
        /// <param name="subscriber"></param>
        private void SubscribeInternal(Subscriber subscriber)
        {
            // check is subscriber is valid
            if(subscriber is not {IsAlive: true})
            {
                _logger.LogError("The {Subscriber} is null or not alive", nameof(subscriber));
                return;
            }

            // capture payload type into local var 'key'
            var key = subscriber.PayloadType;
            // capture subscribers dic into local var 'dic'
            SubscriberSet callbacks;
            if (_subscribersSet.ContainsKey(key))
            {
                // fetch list of callbacks for this payload type
                _subscribersSet.TryGetValue(key, out callbacks);
            }
            else
            {
                // init list of callbacks/subscribers
                callbacks = new SubscriberSet();
                _subscribersSet.Add(key, callbacks);
            }

            if (callbacks == null)
            {
                _logger.LogError($"{nameof(callbacks)} container is null!");
                return;
            }

            // check if subscriber is already registered
            if(callbacks.ContainsKey(subscriber.Id))
            {
                return;
            }
            // register new subscriber
            callbacks.Add(subscriber.Id, subscriber);

            // add new list of callbacks/subscribers into flat list for fast access
            if (!_subscribers.Contains(subscriber))
            {
                _subscribers.Add(subscriber);
            }
        }

        /// <summary>
        /// Unsubscribe given callback by payload type <see cref="T"/>
        /// </summary>
        /// <param name="callback">The callback to unsubscribe</param>
        /// <typeparam name="T">The type of the payload</typeparam>
        /// <exception cref="ArgumentNullException">Exception is thrown in case <param name="callback"></param> is null</exception>
        /// <returns>Instance of <see cref="Messenger"/></returns>
        public IMessengerUnsubscribe Unsubscribe<T>(Action<T> callback) where T : class, new()
        {
            if(callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            // call internal method
            UnsubscribeInternal(typeof(T), callback);
            return this;
        }

        /// <summary>
        /// Unsubscribe given callback by payload type <see cref="TC"/>
        /// </summary>
        /// <param name="callback">The callback to unsubscribe</param>
        /// <typeparam name="TC">The type of the payload</typeparam>
        /// <typeparam name="TS">The type of state object for the given callback</typeparam>
        /// <exception cref="ArgumentNullException">Exception is thrown in case <param name="callback"></param> is null</exception>
        /// <returns>Instance of <see cref="Messenger"/></returns>
        public IMessengerUnsubscribe Unsubscribe<TC, TS>(Action<TC, TS> callback) where TC : class, new()
        {
            if(callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            // call internal method
            UnsubscribeInternal(typeof(TC), callback);
            return this;
        }

        /// <summary>
        /// Unsubscribe given predicate from receiving payload
        /// </summary>
        /// <param name="predicate">The predicate that subscribed to receive payload</param>
        /// <typeparam name="T">Type of predicate to unsubscribe from</typeparam>
        /// <exception cref="ArgumentNullException">Exception is thrown in case <param name="predicate"></param> is null</exception>
        /// <returns>Instance of the Messenger</returns>
        public IMessengerUnsubscribe Unsubscribe<T>(Predicate<T> predicate) where T : class, new()
        {
            if(predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            // call internal method
            UnsubscribeInternal(typeof(T), predicate);
            return this;
        }

        /// <summary>
        /// Unsubscribe given predicate from receiving payload
        /// </summary>
        /// <param name="predicate">The predicate that subscribed to receive payload</param>
        /// <typeparam name="TC">Type of predicate to unsubscribe from</typeparam>
        /// <typeparam name="TS">The type of state object for the given callback</typeparam>
        /// <exception cref="ArgumentNullException">Exception is thrown in case <param name="predicate"></param> is null</exception>
        /// <returns>Instance of the Messenger</returns>
        public IMessengerUnsubscribe Unsubscribe<TC, TS>(Func<TC, TS, bool> predicate) where TC : class, new()
        {
            if(predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            // call internal method
            UnsubscribeInternal(typeof(TC), predicate);
            return this;
        }

        /// <summary>
        /// Unsubscribe given callback by payload type
        /// </summary>
        /// <remarks>Internal method</remarks>
        /// <param name="payloadType">The type of the payload</param>
        /// <param name="callback">The callback delegate</param>
        private void UnsubscribeInternal(Type payloadType, Delegate callback)
        {
            // check if payload is registered
            if (!_subscribersSet.ContainsKey(payloadType))
            {
                return;
            }

            // get list of callbacks for the payload
            _subscribersSet.TryGetValue(payloadType, out var callbacks);
            // check if callbacks list is null or empty and if messenger is publishing payloads
            if(callbacks.IsNullOrEmpty())
            {
                // remove payload from subscribers dic
                _subscribersSet.Remove(payloadType);
                return;
            }

            // get callback ID
            var id = callback.GetHashCode();
            // check if callback is registered
            if(callbacks != null && callbacks.ContainsKey(id))
            {
                // get subscriber instance and dispose it
                var subscriber = callbacks[id];
                subscriber.Dispose();

                // check if messenger is busy with publishing
                if(!callbacks.IsPublishing)
                {
                    // remove the subscriber from the callbacks dic
                    callbacks.Remove(id);
                    // remove the subscriber from the subscribers dic
                    if (_subscribers.Contains(subscriber))
                    {
                        _subscribers.Remove(subscriber);
                    }
                }
            }

            // check is messenger is busy with publishing or if callbacks are NOT empty
            if(callbacks!.IsPublishing || !callbacks.IsNullOrEmpty())
            {
                return;
            }
            // remove callbacks from the _subscribersSet
            _subscribersSet.Remove(payloadType);
        }

        /// <summary>
        /// Process collections.
        /// <para>Cleanup "dead" subscribers and add from waiting list.</para>
        /// </summary>
        private void Process()
        {
            // cleanup "dead" subscribers
            for(var i = 0; i < _subscribers.Count; i++)
            {
                var subscriber = _subscribers[i];
                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                if(subscriber == null)
                {
                    _subscribers.RemoveAt(i);
                    i--;
                    continue;
                }
                if(subscriber.IsAlive)
                {
                    continue;
                }

                _subscribers.Remove(subscriber);
                i--;

                if(!_subscribersSet.ContainsKey(subscriber.PayloadType))
                {
                    continue;
                }

                var callbacks = _subscribersSet[subscriber.PayloadType];
                callbacks?.Remove(subscriber.Id);

                if(callbacks!.Count > 0)
                {
                    continue;
                }
                _subscribersSet.Remove(subscriber.PayloadType);
            }

            // add waiting subscribers
            foreach (var subscriber in _add)
            {
                SubscribeInternal(subscriber);
            }
            _add.Clear();
        }
    }
}

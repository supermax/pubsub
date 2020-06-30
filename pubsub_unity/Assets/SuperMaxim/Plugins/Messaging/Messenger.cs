using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using SuperMaxim.Core.Extensions;
using SuperMaxim.Core.Objects;
using System.Threading;
using SuperMaxim.Core.Threading;
using UnityEngine;
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
        private readonly Dictionary<Type, Dictionary<int, Subscriber>> _subscribersSet = 
                                                new Dictionary<Type, Dictionary<int, Subscriber>>();

        // List of subscribers to optimize iteration during subscribers processing  
        private readonly List<Subscriber> _subscribers = new List<Subscriber>();

        // List of subscribers to optimize add (subscribe) operation 
        private readonly List<Subscriber> _add = new List<Subscriber>();

        // flag, if "true" then do not do changes in "_subscribersSet" dic.
        private bool _isPublishing;

        /// <summary>
        /// Static CTOR to initialize other monitoring tools (singletons)
        /// </summary>
        static Messenger()
        {
            // init MainThreadDispatcher and print main thread ID
            Debug.LogFormat("Main Thread ID: {0}", MainThreadDispatcher.Default.ThreadId);

            // TODO init in case of debug
            // init MessengerMonitor
            Debug.LogFormat("Messenger Monitor {0}", MessengerMonitor.Default); // TODO print id
        }

        /// <summary>
        /// Publish given payload to relevant subscribers
        /// </summary>
        /// <param name="payload">Instance of payload to publish</param>
        /// <typeparam name="T">The type of payload to publish</typeparam>
        /// <returns>Instance of the Messenger</returns>
        public IMessenger Publish<T>(T payload)
        {
            // if calling thread is same as main thread, call "PublishInternal" directly
            if(Thread.CurrentThread.ManagedThreadId == MainThreadDispatcher.Default.ThreadId)
            {
                PublishInternal(payload);
                return this;
            }

            // capture "PublishInternal" in local action var.
            Action<T> act = PublishInternal;
            // add "act" into "MainThreadDispatcher" queue
            MainThreadDispatcher.Default.Dispatch(act, new object[] { payload });
            return this;
        }

        /// <summary>
        /// Publish payload
        /// </summary>
        /// <remarks>
        /// Internal function that is used with "MainThreadDispatcher"
        /// </remarks>
        /// <param name="payload">The payload</param>
        /// <typeparam name="T">The type of the payload</typeparam>
        private void PublishInternal<T>(T payload)
        {
            try
            {
                // turn on the flag
                _isPublishing = true;

                // capture subscribers dic. in local var.
                var dic = _subscribersSet;
                var key = typeof(T); // capture the type of the payload in local var.

                // exit method, if subscribers' dic. does not contain the given payload type
                if (!dic.ContainsKey(key))
                {
                    return;
                }

                // get subscriber's dic. for the payload type
                dic.TryGetValue(key, out var callbacks);
                // check if "callbacks" dic. is null or empty 
                if (callbacks.IsNullOrEmpty())
                {                
                    // remove payload type key is "callbacks" dic is empty
                    dic.Remove(key);
                    return;
                }

                // iterate thru dic and invoke callbacks
                foreach (var callback in callbacks.Values)
                {
                    callback?.Invoke(payload);
                }
            }
            finally
            {
                // turn off the flag
                _isPublishing = false;
                // process pending tasks
                Process();
            }
        }

        public IMessenger Subscribe<T>(Action<T> callback, Predicate<T> predicate = null)
        {
            if(Thread.CurrentThread.ManagedThreadId == MainThreadDispatcher.Default.ThreadId)
            {
                SubscribeInternal(callback, predicate);
                return this;
            }

            Action<Action<T>, Predicate<T>> act = SubscribeInternal;
            MainThreadDispatcher.Default.Dispatch(act, new object[] { callback, predicate });
            return this;
        }

        private void SubscribeInternal<T>(Action<T> callback, Predicate<T> predicate = null)
        {
            var key = typeof(T);
            var id = callback.GetHashCode();            
            var sub = new Subscriber(key, callback, predicate);

            if(_isPublishing)
            {
                _add.Add(sub);
                return;
            }
            SubscribeInternal(sub);
        }

        private void SubscribeInternal(Subscriber subscriber)
        {
            if(subscriber == null || !subscriber.IsAlive)
            {
                return;
            }

            var key = subscriber.PayloadType;
            var dic = _subscribersSet;                        
            Dictionary<int, Subscriber> callbacks;
            if (dic.ContainsKey(key))
            {
                dic.TryGetValue(key, out callbacks);
            }
            else
            {
                callbacks = new Dictionary<int, Subscriber>();
                dic.Add(key, callbacks);
            }

            if (callbacks == null)
            {
                Debug.LogError("callbacks container is null!");
                return;
            }

            if(callbacks.ContainsKey(subscriber.Id))
            {
                return;
            }
            callbacks.Add(subscriber.Id, subscriber);

            if (!_subscribers.Contains(subscriber))
            {
                _subscribers.Add(subscriber);
            }
        }

        public IMessenger Unsubscribe<T>(Action<T> callback)
        {
            if(Thread.CurrentThread.ManagedThreadId == MainThreadDispatcher.Default.ThreadId)
            {
                UnsubscribeInternal(callback);
                return this;
            }

            Action<Action<T>> act = UnsubscribeInternal;
            MainThreadDispatcher.Default.Dispatch(act, new object[] { callback });
            return this;
        }

        private void UnsubscribeInternal<T>(Action<T> callback)
        {
            var key = typeof(T);          
            var dic = _subscribersSet;             
            if (!dic.ContainsKey(key))
            {
                return;
            }

            dic.TryGetValue(key, out var callbacks);
            if(!_isPublishing && callbacks.IsNullOrEmpty())
            {
                dic.Remove(key);
                return;
            }

            var id = callback.GetHashCode();
            if(callbacks.ContainsKey(id))
            {
                var subscriber = callbacks[id];
                subscriber.Dispose();         

                if(!_isPublishing)
                {
                    callbacks.Remove(id);

                    if (_subscribers.Contains(subscriber))
                    {
                        _subscribers.Remove(subscriber);
                    }
                }
            }

            if(_isPublishing || !callbacks.IsNullOrEmpty())
            {
                return;
            }
            dic.Remove(key);
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
                if(subscriber == null || subscriber.IsAlive)
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
                callbacks.Remove(subscriber.Id);

                if(callbacks.Count > 0)
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

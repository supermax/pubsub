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
            Debug.LogFormat("Main Thread ID: {0}", MainThreadDispatcher.Default.MainThreadId);

            // TODO init in case of debug
            // init MessengerMonitor
            Debug.LogFormat("Messenger Monitor {0}", MessengerMonitor.Default); // TODO print id
        }

        public IMessenger Publish<T>(T payload)
        {
            if(Thread.CurrentThread.ManagedThreadId == MainThreadDispatcher.Default.MainThreadId)
            {
                PublishInternal(payload);
                return this;
            }

            // TODO write to log Thread ID
            Action<T> act = PublishInternal;
            MainThreadDispatcher.Default.Dispatch(act, new object[] { payload });
            return this;
        }

        private void PublishInternal<T>(T payload)
        {
            try
            {
                _isPublishing = true;

                var dic = _subscribersSet;
                var key = typeof(T);

                if (!dic.ContainsKey(key))
                {
                    return;
                }

                dic.TryGetValue(key, out var callbacks);
                if (callbacks.IsNullOrEmpty())
                {                
                    dic.Remove(key);
                    return;
                }

                foreach (var callback in callbacks.Values)
                {
                    callback?.Invoke(payload);
                }
            }
            finally
            {
                _isPublishing = false;
                Process();
            }
        }

        public IMessenger Subscribe<T>(Action<T> callback, Predicate<T> predicate = null)
        {
            if(Thread.CurrentThread.ManagedThreadId == MainThreadDispatcher.Default.MainThreadId)
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
            if(Thread.CurrentThread.ManagedThreadId == MainThreadDispatcher.Default.MainThreadId)
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

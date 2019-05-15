using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using SuperMaxim.Core.Extensions;
using SuperMaxim.Core.Objects;
using System.Threading;
using SuperMaxim.Core.Threading;

namespace SuperMaxim.Messaging
{
    public sealed class Messenger : Singleton<IMessenger, Messenger>, IMessenger
    {
        private readonly Dictionary<Type, Dictionary<int, Subscriber>> _subscribersSet = 
                                                new Dictionary<Type, Dictionary<int, Subscriber>>();

        private readonly Dictionary<int, Subscriber> _subscribers = new Dictionary<int, Subscriber>();

        private readonly Dictionary<int, Subscriber> _add = new Dictionary<int, Subscriber>();

        private readonly Dictionary<int, Subscriber> _remove = new Dictionary<int, Subscriber>();

        private bool _isPublishing;

        private static int ThreadId;

        static Messenger()
        {
            ThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        public void Publish<T>(T payload)
        {
            if(Thread.CurrentThread.ManagedThreadId == ThreadId)
            {
                PublishInternal(payload);
                return;
            }

            MainThreadDispatcher.Default.Dispatch(PublishInternal, payload);
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

                Dictionary<int, Subscriber> callbacks;
                dic.TryGetValue(key, out callbacks);
                if (callbacks.IsNullOrEmpty())
                {                
                    dic.Remove(key);
                    return;
                }

                foreach (var callback in callbacks.Values)
                {
                    if (callback == null)
                    {
                        continue;
                    }
                    callback.Invoke(payload);
                }

                // while (callbacks.Count > 0)
                // {
                    
                // }
            }
            finally
            {
                _isPublishing = false;
                Process();
            }
        }

        public void Subscribe<T>(Action<T> callback, Predicate<T> predicate = null)
        {
            if(Thread.CurrentThread.ManagedThreadId == ThreadId)
            {
                SubscribeInternal(callback, predicate);
                return;
            }

            //MainThreadDispatcher.Default.Dispatch(SubscribeInternal, callback);
        }

        private void SubscribeInternal<T>(Action<T> callback, Predicate<T> predicate = null)
        {
            var key = typeof(T);
            if(_isPublishing)
            {
                var id = callback.GetHashCode();
                if(_add.ContainsKey(id))
                {
                    return;
                }
                var sub = new Subscriber(key, callback, predicate);
                _add.Add(id, sub);
                return;
            }

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

            if(callbacks.ContainsKey(callback.GetHashCode()))
            {
                return;
            }

            var weakRef = new Subscriber(key, callback);
            callbacks.Add(weakRef.Id, weakRef);
        }

        public void Unsubscribe<T>(Action<T> callback)
        {
            if(Thread.CurrentThread.ManagedThreadId == ThreadId)
            {
                UnsubscribeInternal(callback);
                return;
            }

            MainThreadDispatcher.Default.Dispatch(UnsubscribeInternal, callback);
        }

        private void UnsubscribeInternal<T>(Action<T> callback)
        {
            var key = typeof(T);          
            var dic = _subscribersSet;             
            if (!dic.ContainsKey(key))
            {
                return;
            }

            Dictionary<int, Subscriber> callbacks;
            dic.TryGetValue(key, out callbacks);

            var id = callback.GetHashCode();
            if(callbacks.ContainsKey(id))
            {
                var wr = callbacks[id];
                wr.Dispose();   

                if(!_isPublishing)
                {                    
                    callbacks.Remove(id);
                }             
            }

            if(!_isPublishing)
            { 
                if(callbacks.Count == 0)
                {
                    dic.Remove(key);
                }
            }
        }

        private void Process()
        {
            if(_isPublishing)
            {                
                return;
            }                     

            foreach (var subscriber in _subscribers.Values)
            {
                if(subscriber.IsAlive)
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
        }
    }
}

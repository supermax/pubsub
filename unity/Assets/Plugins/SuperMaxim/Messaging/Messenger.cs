using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using SuperMaxim.Core.Extensions;
using SuperMaxim.Core.WeakRef;
using SuperMaxim.Core.Objects;
using System.Threading;
using SuperMaxim.Core.Threading;

namespace SuperMaxim.Messaging
{
    public sealed class Messenger : Singleton<IMessenger, Messenger>, IMessenger
    {
        private readonly Dictionary<Type, Dictionary<int, WeakRefDelegate>> _dic = 
                                                new Dictionary<Type, Dictionary<int, WeakRefDelegate>>();

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
            var dic = _dic;
            var key = typeof(T);

            if (!dic.ContainsKey(key))
            {
                return;
            }

            Dictionary<int, WeakRefDelegate> callbacks;
            dic.TryGetValue(key, out callbacks);
            if (callbacks.IsNullOrEmpty())
            {                
                dic.Remove(key);
                return;
            }

            // FIX do not clone, run over captured values
            var ary = new WeakRefDelegate[callbacks.Count];
            callbacks.Values.CopyTo(ary, 0);
            foreach (var callback in ary)
            {
                if (callback == null)
                {
                    continue;
                }
                callback.Invoke(payload);
            }
        }

        public void Subscribe<T>(Action<T> callback)
        {
            if(Thread.CurrentThread.ManagedThreadId == ThreadId)
            {
                SubscribeInternal(callback);
                return;
            }

            MainThreadDispatcher.Default.Dispatch(SubscribeInternal, callback);
        }

        private void SubscribeInternal<T>(Action<T> callback)
        {
            var dic = _dic;
            var key = typeof(T);            

            Dictionary<int, WeakRefDelegate> callbacks;
            if (dic.ContainsKey(key))
            {
                dic.TryGetValue(key, out callbacks);
            }
            else
            {
                callbacks = new Dictionary<int, WeakRefDelegate>();
                dic.Add(key, callbacks);
            }

            if(callbacks.ContainsKey(callback.GetHashCode()))
            {
                return;
            }

            var weakRef = WeakRefDelegate.Create(callback);
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
            // TODO check if publish is iterating, capture value and unsubscribe
            var dic = _dic;
            var key = typeof(T);            

            if (!dic.ContainsKey(key))
            {
                return;
            }

            Dictionary<int, WeakRefDelegate> callbacks;
            dic.TryGetValue(key, out callbacks);

            var id = callback.GetHashCode();
            if(callbacks.ContainsKey(id))
            {
                var wr = callbacks[id];
                wr.Dispose();
                callbacks.Remove(id);                
            }

            if(callbacks.Count == 0)
            {
                dic.Remove(key);
            }
        }

        private void Cleanup()
        {
            throw new NotImplementedException();
        }
    }
}

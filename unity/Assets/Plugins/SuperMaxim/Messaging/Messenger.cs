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
        private readonly ConcurrentDictionary<Type, List<WeakRefDelegate>> _dic = 
                                                new ConcurrentDictionary<Type, List<WeakRefDelegate>>();

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
            List<WeakRefDelegate> list;

            if (!dic.ContainsKey(key))
            {
                return;
            }

            dic.TryGetValue(key, out list);
            if (list.IsNullOrEmpty())
            {
                dic.TryRemove(key, out list);
                return;
            }

            foreach (var callback in list)
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
            var dic = _dic;
            var key = typeof(T);
            List<WeakRefDelegate> list;

            if (dic.ContainsKey(key))
            {
                dic.TryGetValue(key, out list);
            }
            else
            {
                list = new List<WeakRefDelegate>();
                dic.TryAdd(key, list);
            }

            foreach (var wr in list)
            {
                if(wr.Contains(callback))
                {
                    return;
                }
            }
            var weakRef = WeakRefDelegate.Create(callback);
            list.Add(weakRef);

            Cleanup();
        }

        public void Unsubscribe<T>(Action<T> callback)
        {
            var dic = _dic;
            var key = typeof(T);
            List<WeakRefDelegate> list;

            if (!dic.ContainsKey(key))
            {
                return;
            }

            list = dic[key];
            foreach (var wr in list)
            {
                if (wr.Contains(callback))
                {
                    wr.Dispose();
                    list.Remove(wr);
                    break;
                }
            }

            if(list.Count == 0)
            {
                dic.TryRemove(key, out list);
            }
            Cleanup();
        }

        private void Cleanup()
        {
            throw new NotImplementedException();
        }
    }
}

#region

using System;
using System.Collections.Concurrent;
using System.Threading;
using SuperMaxim.Core.Logging;
using SuperMaxim.Core.WeakRef;

#endregion

namespace SuperMaxim.Demo
{
    public class ThreadQueue<T> : IDisposable
    {
        private readonly ConcurrentQueue<Tuple<WeakReferenceDelegate, T>> _queue =
            new ConcurrentQueue<Tuple<WeakReferenceDelegate, T>>();

        private Timer _timer;

        public void Dispose()
        {
            Stop();
            Clear();
        }

        private void OnTick(object state)
        {
            Loggers.Console.LogWarning("OnTick - Thread ID: {0}", Thread.CurrentThread.ManagedThreadId);

            while (_queue.Count > 0)
            {
                if (_queue.TryDequeue(out var tuple))
                {
                    tuple.Item1.Invoke(new object[] {tuple.Item2});
                }
            }
        }

        public void Start()
        {
            _timer = new Timer(OnTick, null, 0, 1000);
        }

        public void Stop()
        {
            if (_timer == null) return;

            _timer.Dispose();
            _timer = null;
        }

        public void Clear()
        {
            while (_queue.Count > 0)
            {
                if (_queue.TryDequeue(out var tuple))
                {
                    tuple.Item1.Dispose();
                }
            }
        }

        public void Enqueue(Action<T> method, T parameter)
        {
            _queue.Enqueue(new Tuple<WeakReferenceDelegate, T>(new WeakReferenceDelegate(method), parameter));
        }
    }
}

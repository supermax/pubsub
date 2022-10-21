#if UNITY
using System;
using System.Collections.Concurrent;
using System.Threading;
using SuperMaxim.Core.Logging;
using SuperMaxim.Core.Objects;

namespace SuperMaxim.Core.Threading
{
    public class UnityMainThreadDispatcher 
        : MonoBehaviourSingleton<IThreadDispatcher, UnityMainThreadDispatcher>
            , IThreadDispatcher
    {
        private readonly ConcurrentQueue<DispatcherTask> _tasks = new ConcurrentQueue<DispatcherTask>();

        public int ThreadId
        {
            get;
            private set;
        }

        public int TasksCount
        {
            get
            {
                return _tasks.Count;
            }
        }

        private void Awake()
        {
            ThreadId = Thread.CurrentThread.ManagedThreadId;                
        }

        public void Dispatch(Delegate action, object[] payload)
        {
            _tasks.Enqueue(new DispatcherTask(action, payload));
        }

        private void Update()
        {
            while(_tasks.Count > 0)
            {
                if (!_tasks.TryDequeue(out var task))
                {
                    continue;
                }
                Loggers.Console.LogInfo("(Queue.Count: {0}) Dispatching task {1}", _tasks.Count, task.Action);

                task.Invoke();
                task.Dispose();
            }
        }
    }
}
#endif

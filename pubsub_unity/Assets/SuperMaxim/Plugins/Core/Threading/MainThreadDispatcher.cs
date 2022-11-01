using System;
using System.Threading;
using SuperMaxim.Core.Objects;
using System.Collections.Concurrent;
using SuperMaxim.Core.Logging;

namespace SuperMaxim.Core.Threading
{
    public class MainThreadDispatcher 
        : Singleton<IThreadDispatcher, MainThreadDispatcher>
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

        public MainThreadDispatcher()
        {
            ThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        public void Dispatch(Delegate action, object[] payload)
        {
            _tasks.Enqueue(new DispatcherTask(action, payload));
        }

        // TODO adjust for classic .NET async queue dispatching
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
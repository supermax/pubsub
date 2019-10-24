using System;
using System.Threading;
using SuperMaxim.Core.Objects;
using System.Collections.Concurrent;
using UnityEngine;
using System.Collections;

namespace SuperMaxim.Core.Threading
{
    public class MainThreadDispatcher : 
                    MonoBehaviourSingleton<IThreadDispatcher, MainThreadDispatcher>
                    , IThreadDispatcher
    {
        private readonly ConcurrentQueue<DispatcherTask> _tasks = new ConcurrentQueue<DispatcherTask>();

        public int MainThreadId
        {
            get;
            private set;
        }

        public int TasksCount
        {
            get { return _tasks.Count; }
        }

        private void Awake()
        {
            MainThreadId = Thread.CurrentThread.ManagedThreadId;                
        }

        public void Dispatch(Delegate action, object[] payload)
        {
            _tasks.Enqueue(new DispatcherTask(action, payload));
        }

        private void Update()
        {
            while(_tasks.Count > 0)
            {
                DispatcherTask task;
                if(_tasks.TryDequeue(out task))
                {
                    // TODO temove this temp log
                    Debug.LogFormat("(Queue.Count: {0}) Dispatching task {1}", _tasks.Count, task.Action);

                    task.Invoke();
                    task.Dispose();
                }                
            }
        }
    }
}
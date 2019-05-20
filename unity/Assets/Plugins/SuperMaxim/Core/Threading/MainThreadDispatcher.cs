using System;
using System.Collections.Generic;
using System.Threading;
using SuperMaxim.Core.Objects;
using UnityEngine;

namespace SuperMaxim.Core.Threading
{
    public class MainThreadDispatcher : MonoBehaviourSingleton<IThreadDispatcher, MainThreadDispatcher>, IThreadDispatcher
    {
        private readonly Queue<DispatcherAction> _actions = new Queue<DispatcherAction>();

        public int MainThreadId
        {
            get
            {
                var id = Thread.CurrentThread.ManagedThreadId;
                return id;
            }
        }

        public void Dispatch(Delegate action, object[] payload)
        {
            _actions.Enqueue(new DispatcherAction(action, payload));
        }

        private void Update()
        {
            while(_actions.Count > 0)
            {
                var action = _actions.Dequeue();
                action.Invoke();
                action.Dispose();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using SuperMaxim.Core.Objects;
using SuperMaxim.Core.WeakRef;
using UnityEngine;

namespace SuperMaxim.Core.Threading
{
    public class MainThreadDispatcher : MonoBehaviourSingleton<IThreadDispatcher, MainThreadDispatcher>, IThreadDispatcher
    {
        private readonly Stack<Tuple<WeakRefDelegate, WeakRefWrapper>> _actions = 
                                        new Stack<Tuple<WeakRefDelegate, WeakRefWrapper>>();

        public void Dispatch<T>(Action<T> action, T payload)
        {
            //_actions.Push(new Tuple<WeakRefDelegate, WeakRefWrapper>(action, payload));
        }

        public void DispatchRoutine<T>(Action<T> action, T payload, float repeatTime)
        {
            
        }

        private void Update()
        {
            var tuple = _actions.Pop();
            tuple.Item1.Invoke(tuple.Item2.Target);
            if(!tuple.Item1.IsAlive)
            {
                tuple.Item1.Dispose();
            }
        }
    }
}
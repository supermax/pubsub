using System;
using UnityEngine;

namespace SuperMaxim.Core.Threading
{
    public class MainThreadDispatcher : MonoBehaviour, IThreadDispatcher
    {
        public void Dispatch<T>(Action<T> action, T payload)
        {
            throw new NotImplementedException();
        }

        public void DispatchRoutine<T>(Action<T> action, T payload, float repeatTime)
        {
            throw new NotImplementedException();
        }
    }
}
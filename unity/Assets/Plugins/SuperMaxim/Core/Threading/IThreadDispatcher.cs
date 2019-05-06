using System;

namespace SuperMaxim.Core.Threading
{
    public interface IThreadDispatcher
    {
        void Dispatch<T>(Action<T> action, T payload);

        void DispatchRoutine<T>(Action<T> action, T payload, float repeatTime);
    }
}

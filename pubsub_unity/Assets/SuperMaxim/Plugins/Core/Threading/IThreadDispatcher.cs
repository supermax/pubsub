using System;

namespace SuperMaxim.Core.Threading
{
    public interface IThreadDispatcher
    {
        int MainThreadId { get; }

        void Dispatch(Delegate action, object[] payload);        
    }
}

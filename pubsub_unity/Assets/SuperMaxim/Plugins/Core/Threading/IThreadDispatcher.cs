using System;

namespace SuperMaxim.Core.Threading
{
    public interface IThreadDispatcher
    {
        int ThreadId { get; }

        void Dispatch(Delegate action, object[] payload);        
    }
}

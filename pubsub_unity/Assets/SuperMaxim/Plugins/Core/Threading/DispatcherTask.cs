using System;
using SuperMaxim.Core.WeakRef;

namespace SuperMaxim.Core.Threading
{
    public class DispatcherTask : IDisposable
    {
        public WeakReferenceDelegate Action 
        {
            get; private set;
        }

        private object[] Payload
        {
            get;
            set;
        }

        public DispatcherTask(Delegate action, object[] payload)
        {
            Action = new WeakReferenceDelegate(action);
            Payload = payload;
        }

        public void Invoke()
        {
            if(Action == null || !Action.IsAlive)
            {
                return;
            }
            Action.Invoke(Payload);            
        }

        public void Dispose()
        {
            if(Action != null)
            {
                Action.Dispose();
                Action = null;
            }
            Payload = null;            
        }
    }    
}

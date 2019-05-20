using System;
using SuperMaxim.Core.Extensions;
using SuperMaxim.Core.WeakRef;

namespace SuperMaxim.Core.Threading
{
    public class DispatcherAction : IDisposable
    {
        public WeakRefDelegate Action 
        {
            get; private set;
        }

        public object[] Payload
        {
            get; private set;
        }

        public DispatcherAction(Delegate action, object[] payload)
        {
            Action = new WeakRefDelegate(action);
            Payload = payload;
        }

        public void Invoke()
        {
            if(Action == null || !Action.IsAlive)
            {
                return;
            }

            if(Payload == null)
            {
                Action.Invoke();
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
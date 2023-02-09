using System.Collections.Generic;

namespace SuperMaxim.Messaging
{
    internal class SubscriberSet : Dictionary<int, Subscriber>
    {
        internal bool IsPublishing { get; private set; }

        internal void Publish(object payload)
        {
            IsPublishing = true;
            try
            {
                // iterate thru list of subscribers and invoke type's predicate
                foreach (var callback in Values)
                {
                    if (callback is not {IsPredicate: true})
                    {
                        continue;
                    }
                    // if type's predicate returns 'false' abort publish method
                    var res = callback.Invoke(payload);
                    if (res != null && !(bool)res)
                    {
                        return;
                    }
                }

                // iterate thru list of subscribers and invoke callbacks
                foreach (var callback in Values)
                {
                    if (callback.IsPredicate)
                    {
                        continue;
                    }
                    callback.Invoke(payload);
                }
            }
            finally
            {
                IsPublishing = false;
            }
        }
    }
}

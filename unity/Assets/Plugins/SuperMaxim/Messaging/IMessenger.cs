using System;

namespace SuperMaxim.Messaging
{
    public interface IMessenger
    {
        void Publish<T>(T payload);

        void Subscribe<T>(Action<T> callback);

        void Unsubscribe<T>(Action<T> callback);
    }
}

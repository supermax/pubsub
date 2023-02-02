using SuperMaxim.Messaging;

namespace SuperMaxim.Messaging.API
{
    public interface IMessengerSubscriber
    {
        IMessenger Messenger { get; }

        IMessengerSubscriber SubscribeAll();

        IMessengerSubscriber UnsubscribeAll();
    }
}

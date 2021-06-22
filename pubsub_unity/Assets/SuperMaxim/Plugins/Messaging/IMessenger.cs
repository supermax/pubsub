using System;

namespace SuperMaxim.Messaging
{
    /// <summary>
    /// Interface for Pub/Sub Messenger
    /// </summary>
    public interface IMessenger : IMessengerPublish, IMessengerSubscribe, IMessengerUnsubscribe
    {
        
    }
}

namespace SuperMaxim.Messaging.API
{
    /// <summary>
    /// Interface for Pub/Sub Messenger
    /// </summary>
    public interface IMessenger : IMessengerPublish, IMessengerSubscribe, IMessengerUnsubscribe
    {

    }
}

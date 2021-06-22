namespace SuperMaxim.Messaging
{
    /// <summary>
    /// Interface for Publishing
    /// </summary>
    public interface IMessengerPublish
    {
        /// <summary>
        /// Publish given payload to relevant subscribers
        /// </summary>
        /// <param name="payload">Instance of payload to publish</param>
        /// <typeparam name="T">The type of payload to publish</typeparam>
        /// <returns>Instance of the Messenger</returns>
        IMessengerPublish Publish<T>(T payload);
    }
}
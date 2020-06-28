using System;

namespace SuperMaxim.Messaging
{
    /// <summary>
    /// Interface for Pub/Sub Messenger
    /// </summary>
    public interface IMessenger
    {
        /// <summary>
        /// Publish given payload to relevant subscribers
        /// </summary>
        /// <param name="payload">Instance of payload to publish</param>
        /// <typeparam name="T">The type of payload to publish</typeparam>
        /// <returns>Instance of the Messenger</returns>
        IMessenger Publish<T>(T payload);

        /// <summary>
        /// Subscribe given callback to receive payload
        /// </summary>
        /// <param name="callback">The callback that will receive the payload</param>
        /// <param name="predicate">The predicate to filter irrelevant payloads (optional)</param>
        /// <typeparam name="T">The type of payload to receive</typeparam>
        /// <returns>Instance of the Messenger</returns>
        IMessenger Subscribe<T>(Action<T> callback, Predicate<T> predicate = null);

        /// <summary>
        /// Unsubscribe given callback from receiving payload  
        /// </summary>
        /// <param name="callback">The callback that subscribed to receive payload</param>
        /// <typeparam name="T">Type of payload to unsubscribe from</typeparam>
        /// <returns>Instance of the Messenger</returns>
        IMessenger Unsubscribe<T>(Action<T> callback);
    }
}

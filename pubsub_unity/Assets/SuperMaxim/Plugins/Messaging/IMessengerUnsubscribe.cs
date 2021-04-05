using System;

namespace SuperMaxim.Messaging
{
    /// <summary>
    /// Interface for Unsubscription
    /// </summary>
    public interface IMessengerUnsubscribe
    {
        /// <summary>
        /// Unsubscribe given callback from receiving payload  
        /// </summary>
        /// <param name="callback">The callback that subscribed to receive payload</param>
        /// <typeparam name="T">Type of payload to unsubscribe from</typeparam>
        /// <returns>Instance of the Messenger</returns>
        IMessengerUnsubscribe Unsubscribe<T>(Action<T> callback);
    }
}
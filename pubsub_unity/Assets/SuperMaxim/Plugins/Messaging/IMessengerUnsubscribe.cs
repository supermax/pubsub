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
        
        /// <summary>
        /// Unsubscribe given predicate from receiving payload  
        /// </summary>
        /// <param name="predicate">The predicate that subscribed to receive payload</param>
        /// <typeparam name="T">Type of predicate to unsubscribe from</typeparam>
        /// <returns>Instance of the Messenger</returns>
        IMessengerUnsubscribe Unsubscribe<T>(Predicate<T> predicate);
    }
}
using System;

namespace SuperMaxim.Messaging
{
    /// <summary>
    /// Interface for Subscription
    /// </summary>
    public interface IMessengerSubscribe
    {
        /// <summary>
        /// Subscribe given callback to receive payload
        /// </summary>
        /// <param name="callback">The callback that will receive the payload</param>
        /// <param name="predicate">Callback's predicate</param>
        /// <typeparam name="T">The type of payload to receive</typeparam>
        /// <returns>Instance of the Messenger</returns>
        IMessengerSubscribe Subscribe<T>(Action<T> callback, Predicate<T> predicate = null);
        
        // /// <summary>
        // /// The predicate to filter irrelevant payloads (optional)
        // /// </summary>
        // /// <param name="predicate">The predicate to filter irrelevant payloads (optional)</param>
        // /// <typeparam name="T">The type of payload to receive</typeparam>
        // /// <returns>Instance of the Messenger</returns>
        // IMessengerSubscribe Predicate<T>(Predicate<T> predicate);
    }
}
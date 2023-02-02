using System;

namespace SuperMaxim.Messaging.API
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
        IMessengerSubscribe Subscribe<T>(Action<T> callback, Predicate<T> predicate = null) where T : class, new();

        /// <summary>
        /// Subscribe given callback to receive payload with state object
        /// </summary>
        /// <param name="callback">The callback that will receive the payload</param>
        /// <param name="predicate">Callback's predicate</param>
        /// <param name="stateObj">The state object</param>
        /// <typeparam name="TC">The type of payload to receive for the given callback</typeparam>
        /// <typeparam name="TS">The type of state object to receive for the given callback</typeparam>
        /// <returns>Instance of the Messenger</returns>
        IMessengerSubscribe Subscribe<TC, TS>(Action<TC, TS> callback, Func<TC, TS, bool> predicate = null, TS stateObj = default) where TC : class, new();

        /// <summary>
        /// Subscribe predicate to filter irrelevant payloads per given type <typeparam name="T"/>
        /// </summary>
        /// <param name="predicate">The predicate to filter irrelevant payloads</param>
        /// <typeparam name="T">The type of payload to receive</typeparam>
        /// <returns>Instance of the Messenger</returns>
        IMessengerSubscribe Subscribe<T>(Predicate<T> predicate) where T : class, new();

        /// <summary>
        /// Subscribe predicate to filter irrelevant payloads per given type <typeparam name="TC"/>
        /// </summary>
        /// <param name="predicate">The predicate to filter irrelevant payloads</param>
        /// <param name="stateObj">The state object</param>
        /// <typeparam name="TC">The type of payload to receive</typeparam>
        /// <typeparam name="TS">The type of state object to receive for the given callback</typeparam>
        /// <returns>Instance of the Messenger</returns>
        IMessengerSubscribe Subscribe<TC, TS>(Func<TC, TS, bool> predicate, TS stateObj = default) where TC : class, new();
    }
}

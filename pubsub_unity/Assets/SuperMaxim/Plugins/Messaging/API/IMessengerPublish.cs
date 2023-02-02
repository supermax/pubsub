using System;

namespace SuperMaxim.Messaging.API
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
        IMessengerPublish Publish<T>(T payload) where T : class, new();

        /// <summary>
        /// Publish payload
        /// </summary>
        /// <param name="payloadType">The type of the payload</param>
        /// <param name="payload">The payload</param>
        /// <exception cref="ArgumentNullException">Exception is thrown in case <param name="payloadType"></param> is null</exception>
        /// <exception cref="ArgumentNullException">Exception is thrown in case <param name="payload"></param> is null</exception>
        /// <exception cref="InvalidCastException">Exception is thrown in case <param name="payload"></param> type doesn't match <param name="payloadType"></param></exception>
        /// <returns>Instance of the Messenger</returns>
        IMessengerPublish Publish(Type payloadType, object payload);
    }
}

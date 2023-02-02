using System.Diagnostics;
using NUnit.Framework;

namespace SuperMaxim.Tests.Messaging.Fixtures
{
    public class MessengerWeakReferenceCallback
    {
        // ReSharper disable once MemberCanBeMadeStatic.Global (SHOULD NOT BE STATIC!)
        public void Callback(MessengerTestPayload<int> payload)
        {
            Assert.That(payload, Is.Not.Null);
            Debug.WriteLine("[{0}] Object Payload Id: {1}", nameof(MessengerWeakReferenceCallback), payload.Id);
        }
    }
}

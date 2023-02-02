using System.Diagnostics;
using System.Threading.Tasks;
using NUnit.Framework;
using SuperMaxim.Tests.Messaging.Fixtures;

namespace SuperMaxim.Tests.Messaging
{
    [TestFixture]
    public class MessengerWeakReferenceTest : BaseMessengerTest
    {
        private readonly Reference<MessengerWeakReferenceCallback> _weakRefTestCallbackHolder;

        public MessengerWeakReferenceTest()
        {
            _weakRefTestCallbackHolder = new Reference<MessengerWeakReferenceCallback> {Ref = new()};
        }

        [Test]
        public async Task TestWeakReferenceCallback()
        {
            Assert.That(Messenger, Is.Not.Null);
            Assert.That(_weakRefTestCallbackHolder, Is.Not.Null);
            Assert.That(_weakRefTestCallbackHolder.Ref, Is.Not.Null);

            Debug.Assert(_weakRefTestCallbackHolder.Ref != null
                , "{0}.{1} != null"
                , nameof(_weakRefTestCallbackHolder)
                , _weakRefTestCallbackHolder.Ref);
            var instance1 = Messenger.Subscribe<MessengerTestPayload<int>>(_weakRefTestCallbackHolder.Ref.Callback);
            Assert.That(instance1, Is.Not.Null);
            Assert.That(Messenger, Is.SameAs(instance1));

            var payload = new MessengerTestPayload<int>{ Id = 12345 };
            Debug.WriteLine("[{0}] #1 Publish Payload Id: {1}", nameof(TestWeakReferenceCallback), payload.Id);

            var instance2 = Messenger.Publish(new MessengerTestPayload<int>{ Id = 12345 });
            Assert.That(instance2, Is.Not.Null);
            Assert.That(instance2, Is.SameAs(instance1));

            _weakRefTestCallbackHolder.Dispose();

            await Task.Run(() => {
                Debug.WriteLine("[{0}] #2 Publish Payload Id: {1}, {2}: {3}"
                    , nameof(TestWeakReferenceCallback)
                    , payload.Id
                    , nameof(_weakRefTestCallbackHolder.IsDisposed)
                    , _weakRefTestCallbackHolder.IsDisposed);

                var instance3 = Messenger.Publish(payload);

                Assert.That(instance3, Is.Not.Null);
                Assert.That(instance2, Is.SameAs(instance3));
            });
        }
    }
}

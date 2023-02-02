using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NUnit.Framework;
using SuperMaxim.Tests.Messaging.Fixtures;

namespace SuperMaxim.Tests.Messaging
{
    [TestFixture]
    public class MessengerMultithreadingTest : BaseMessengerTest
    {
        [Test]
        public async Task TestPublishFromNewThread()
        {
            Assert.That(Messenger, Is.Not.Null);

            var instance = Messenger.Subscribe<MessengerTestPayload<int>>(OnPublishFromNewThreadCallback);
            Assert.That(instance, Is.Not.Null);
            Assert.That(Messenger, Is.SameAs(instance));
            Debug.WriteLine($"{nameof(Environment.CurrentManagedThreadId)}: {Environment.CurrentManagedThreadId}");

            void Action() => PublishFromNewThreadMethod(Environment.CurrentManagedThreadId);
            await Task.Run(Action);
        }

        private void PublishFromNewThreadMethod(object threadIdObj)
        {
            Assert.That(Messenger, Is.Not.Null);

            var threadId = (int)threadIdObj;

            Debug.WriteLine($"{nameof(Environment.CurrentManagedThreadId)}: {Environment.CurrentManagedThreadId}" +
                            $", {nameof(threadId)}: {threadId}");
            Assert.That(Environment.CurrentManagedThreadId, Is.EqualTo(threadId));

            Messenger.Publish(new MessengerTestPayload<int>{ Data = Environment.CurrentManagedThreadId });
        }

        private void OnPublishFromNewThreadCallback(MessengerTestPayload<int> payload)
        {
            Debug.WriteLine($"[{nameof(OnPublishFromNewThreadCallback)}] Int Payload: {0} (Thread ID: {1})",
                payload.Data, Environment.CurrentManagedThreadId);

            Assert.That(Environment.CurrentManagedThreadId, Is.EqualTo(payload.Data));
        }

        [Test]
        [TestCase(4, Category = "Async Publish", Description = "Opening X threads to publish string payload")]
        public async Task TestPublishAsync(int count)
        {
            Assert.That(Messenger, Is.Not.Null);
            Assert.That(count, Is.GreaterThan(0));

            for (var i = 1; i <= count; i++)
            {
                await Task.Run(() =>
                {
                    var instance = Messenger.Publish(new MessengerTestPayload<string>{Data = $"Hello World! [{i}]"});
                    Assert.That(instance, Is.Not.Null);
                });
            }
        }
    }
}

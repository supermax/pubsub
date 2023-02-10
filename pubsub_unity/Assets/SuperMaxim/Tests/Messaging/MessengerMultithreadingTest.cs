using System;
using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;
using SuperMaxim.Tests.Messaging.Fixtures;
using UnityEngine;
using UnityEngine.TestTools;

namespace SuperMaxim.Tests.Messaging
{
    [TestFixture]
    public class MessengerMultithreadingTest : BaseMessengerTest
    {
        [UnityTest]
        public IEnumerator TestPublishFromNewThread()
        {
            Assert.That(Messenger, Is.Not.Null);

            var instance = Messenger.Subscribe<MessengerTestPayload<int>>(OnPublishFromNewThreadCallback);
            Assert.That(instance, Is.Not.Null);
            Assert.That(Messenger, Is.SameAs(instance));
            Debug.LogFormat($"{nameof(Environment.CurrentManagedThreadId)}: {Environment.CurrentManagedThreadId}");

            void Action() => PublishFromNewThreadMethod(Environment.CurrentManagedThreadId);
            Task.Run(Action);

            yield return new WaitForEndOfFrame();
        }

        private void PublishFromNewThreadMethod(object threadIdObj)
        {
            Assert.That(Messenger, Is.Not.Null);

            var threadId = (int)threadIdObj;

            Debug.LogFormat($"{nameof(Environment.CurrentManagedThreadId)}: {Environment.CurrentManagedThreadId}" +
                            $", {nameof(threadId)}: {threadId}");
            Assert.That(Environment.CurrentManagedThreadId, Is.EqualTo(threadId));

            Messenger.Publish(new MessengerTestPayload<int>{ Data = Environment.CurrentManagedThreadId });
        }

        private void OnPublishFromNewThreadCallback(MessengerTestPayload<int> payload)
        {
            Debug.LogFormat($"[{nameof(OnPublishFromNewThreadCallback)}] Int Payload: {0} (Thread ID: {1})",
                payload.Data, Environment.CurrentManagedThreadId);

            Assert.That(Environment.CurrentManagedThreadId, Is.EqualTo(payload.Data));
        }

        [UnityTest]
        public IEnumerator TestPublishAsync()
        {
            Assert.That(Messenger, Is.Not.Null);

            var wait = new WaitForEndOfFrame();
            for (var i = 1; i <= 4; i++)
            {
                var count = i;
                Task.Run(() =>
                {
                    var instance = Messenger.Publish(new MessengerTestPayload<string>{Data = $"Hello World! [{count}]"});
                    Assert.That(instance, Is.Not.Null);
                });
                yield return wait;
            }
        }
    }
}

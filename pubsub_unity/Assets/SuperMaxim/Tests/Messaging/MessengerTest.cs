using System.Collections;
using System.Threading;
using NUnit.Framework;
using SuperMaxim.Logging;
using SuperMaxim.Messaging;
using UnityEngine;
using UnityEngine.TestTools;

namespace SuperMaxim.Tests.Messaging
{
    public class MessengerTest
    {
        [Test]
        public void SubscribeToStringPayload()
        {
            Messenger.Default.Subscribe<string>(OnStringCallback);
        }

        [UnityTest]
        public IEnumerator PublishAsync()
        {
            for (var i = 1; i <= 4; i++)
            {
                Messenger.Default.Publish($"Hello World! [{i}]");

                yield return null;    
            }
        }

        [Test]
        public void SubscribeAndPublishToObjectWithPredicate()
        {
            Messenger.Default
                .Subscribe<MessengerTestPayload>(OnSubscribeToObjectWithPredicate, ObjectPredicate);

            Messenger.Default.Publish(new MessengerTestPayload{ Id = Random.Range(-1, 1)});
        }

        private static bool ObjectPredicate(MessengerTestPayload payload)
        {
            var accepted = payload.Id % 2 == 0;
            Loggers.Console.LogInfo("[ObjectPredicate] Object Payload Id: {0}, Accepted: {1}", payload.Id, accepted);
            return accepted;
        }

        private static void OnSubscribeToObjectWithPredicate(MessengerTestPayload payload)
        {
            Loggers.Console.LogInfo("[OnSubscribeToObjectWithPredicate] Object Payload Id: {0}", payload.Id);
        }

        private class MessengerTestPayload
        {
            public int Id { get; set; }
        }

        private static void OnStringCallbackStatic(string str)
        {
            Loggers.Console.LogInfo("[OnStringCallbackStatic] String Payload: {0}", str);

            Messenger.Default.Unsubscribe<string>(OnStringCallbackStatic);
        }

        private void OnStringCallback(string str)
        {
            Loggers.Console.LogInfo("[OnStringCallback] String Payload: {0}", str);

            Messenger.Default.Unsubscribe<string>(OnStringCallback);
            Messenger.Default.Subscribe<string>(OnStringCallbackStatic);
        }

        [Test]
        public void PublishFromNewThread()
        {
            Messenger.Default.Subscribe<int>(OnPublishFromNewThreadCallback);

            Loggers.Console.LogInfo("[MessengerTestPublishFromNewThread] Thread ID: {0}", 
                                Thread.CurrentThread.ManagedThreadId);

            var th = new Thread(PublishFromNewThreadMethod);
            th.Start();
        }

        private void PublishFromNewThreadMethod()
        {
            Loggers.Console.LogInfo("[PublishFromNewThread] Thread ID: {0}", 
                                Thread.CurrentThread.ManagedThreadId);

            Messenger.Default.Publish(Thread.CurrentThread.ManagedThreadId);
        }

        private void OnPublishFromNewThreadCallback(int number)
        {
            Loggers.Console.LogInfo("[OnPublishFromNewThreadCallback] Int Payload: {0} (Thread ID: {1})", 
                                number, Thread.CurrentThread.ManagedThreadId);

            Assert.AreNotEqual(number, Thread.CurrentThread.ManagedThreadId);
        }

        private MessengerWekRefTest _weakRefTest;

        [UnityTest]
        public IEnumerator TestWeakReference()
        {
            _weakRefTest = new MessengerWekRefTest();
            Messenger.Default.Subscribe<MessengerTestPayload>(_weakRefTest.Callback);

            var payload = new MessengerTestPayload{ Id = 12345 };
            Loggers.Console.LogInfo("[TestWeakReference] #1 Publish Payload Id: {0}", payload.Id);

            Messenger.Default.Publish(new MessengerTestPayload{ Id = 12345 });

            _weakRefTest = null;

            yield return new WaitForSeconds(5);

            Loggers.Console.LogInfo("[TestWeakReference] #2 Publish Payload Id: {0}", payload.Id);
            Messenger.Default.Publish(payload);
        }

        private class MessengerWekRefTest
        {
            public void Callback(MessengerTestPayload payload)
            {
                Loggers.Console.LogInfo("[MessengerWekRefTest.Callback] Object Payload Id: {0}", payload.Id);
            }
        }
    }
}

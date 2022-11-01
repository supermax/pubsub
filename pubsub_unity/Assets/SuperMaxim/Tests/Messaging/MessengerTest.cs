using System.Collections;
using System.Threading;
using NUnit.Framework;
using SuperMaxim.Core.Logging;
using SuperMaxim.Core.Threading;
using SuperMaxim.Messaging;
using UnityEngine;
using UnityEngine.TestTools;

namespace SuperMaxim.Tests.Messaging
{
    public class MessengerTest
    {
        private class FilteredPayload
        {
            public bool IsFilterOn { get; set; }
        }
        
        [OneTimeSetUp]
        public void Setup()
        {
            var instance1 = Messenger.Default.Subscribe<FilteredPayload>(FilterPayloadCondition);
            Assert.NotNull(instance1);
            
            var instance2 = Messenger.Default.Subscribe<FilteredPayload>(OnFilteredPayloadCallback);
            Assert.NotNull(instance2);
            Assert.AreSame(instance1, instance2);
        }

        private static bool FilterPayloadCondition(FilteredPayload payload)
        {
            Loggers.Console.LogInfo($"{nameof(payload.IsFilterOn)}: {{0}}", payload.IsFilterOn);
            return !payload.IsFilterOn;
        }

        [Test]
        public void Test_PublishFilteredPayload()
        {
            var instance1 = Messenger.Default.Publish(new FilteredPayload {IsFilterOn = true});
            Assert.NotNull(instance1);
            
            var instance2 = Messenger.Default.Publish(new FilteredPayload {IsFilterOn = false});
            Assert.NotNull(instance2);
            Assert.AreSame(instance1, instance2);
        }

        private void OnFilteredPayloadCallback(FilteredPayload payload)
        {
            Loggers.Console.LogInfo($"{nameof(payload)}.{nameof(payload.IsFilterOn)}:{{0}}", payload?.IsFilterOn);
            
            Assert.NotNull(payload);
            Assert.True(!payload.IsFilterOn);
        }

        [Test]
        public void Test_SubscribeToStringPayload()
        {
            var instance = Messenger.Default.Subscribe<string>(OnStringCallback);
            Assert.NotNull(instance);
        }

        [UnityTest]
        public IEnumerator Test_PublishAsync()
        {
            for (var i = 1; i <= 4; i++)
            {
                var instance = Messenger.Default.Publish($"Hello World! [{i}]");
                Assert.NotNull(instance);

                yield return null;    
            }
        }

        [Test]
        public void Test_SubscribeAndPublishToObjectWithPredicate()
        {
            var instance1 = Messenger.Default.Subscribe<MessengerTestPayload>(OnSubscribeToObjectWithPredicate, ObjectPredicate);
            Assert.NotNull(instance1);

            var instance2 = Messenger.Default.Publish(new MessengerTestPayload{ Id = Random.Range(-1, 1)});
            Assert.NotNull(instance2);
            Assert.AreSame(instance1, instance2);
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
        public void Test_PublishFromNewThread()
        {
            var instance = Messenger.Default.Subscribe<int>(OnPublishFromNewThreadCallback);
            Assert.NotNull(instance);

            Loggers.Console.LogInfo($"{nameof(UnityMainThreadDispatcher.Default.ThreadId)}: {{0}}, " +
                                    $"{nameof(Thread.CurrentThread.ManagedThreadId)}: {{1}}"
                , UnityMainThreadDispatcher.Default.ThreadId
                , Thread.CurrentThread.ManagedThreadId);
            Assert.AreEqual(UnityMainThreadDispatcher.Default.ThreadId, Thread.CurrentThread.ManagedThreadId);

            var th = new Thread(PublishFromNewThreadMethod);
            th.Start();
        }

        private void PublishFromNewThreadMethod()
        {
            Loggers.Console.LogInfo($"{nameof(UnityMainThreadDispatcher.Default.ThreadId)}: {{0}}, " +
                                    $"{nameof(Thread.CurrentThread.ManagedThreadId)}: {{1}}"
                , UnityMainThreadDispatcher.Default.ThreadId
                , Thread.CurrentThread.ManagedThreadId);
            Assert.AreNotEqual(UnityMainThreadDispatcher.Default.ThreadId, Thread.CurrentThread.ManagedThreadId);
            
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
        public IEnumerator Test_TestWeakReference()
        {
            _weakRefTest = new MessengerWekRefTest();
            var instance1 = Messenger.Default.Subscribe<MessengerTestPayload>(_weakRefTest.Callback);
            Assert.NotNull(instance1);

            var payload = new MessengerTestPayload{ Id = 12345 };
            Loggers.Console.LogInfo("[TestWeakReference] #1 Publish Payload Id: {0}", payload.Id);

            var instance2 = Messenger.Default.Publish(new MessengerTestPayload{ Id = 12345 });
            Assert.NotNull(instance2);
            Assert.AreSame(instance1, instance2);

            _weakRefTest = null;

            yield return new WaitForSeconds(5);

            Loggers.Console.LogInfo("[TestWeakReference] #2 Publish Payload Id: {0}", payload.Id);
            var instance3 = Messenger.Default.Publish(payload);
            Assert.NotNull(instance3);
            Assert.AreSame(instance3, instance2);
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

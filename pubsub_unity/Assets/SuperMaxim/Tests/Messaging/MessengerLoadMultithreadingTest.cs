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
    public class MessengerLoadMultithreadingTest : BaseMessengerTest
    {
        private const int SendMessageChunksCount = 10;

        private const int SendMessagesCount = 10;

        private int _receivedCount;

        private double _totalTime;

        private TimeSpan _loopTime;

        private Task _testThread;

        private readonly LoadTestPayload _payload = new();

        [UnityTest]
        public IEnumerator TestLoadAsync()
        {
            Assert.That(Messenger, Is.Not.Null);
            var instance = Messenger.Subscribe<LoadTestPayload>(OnTestCallback);
            Assert.That(instance, Is.SameAs(Messenger));

            _testThread = new Task(RunLoadLoop);
            _testThread.Start();

            var wait = new WaitForSeconds(1);
            while (_testThread.Status == TaskStatus.Running)
            {
                yield return wait;
            }
        }

        private void RunLoadLoop()
        {
            _totalTime = 0.0;
            for (var i = 0; i < SendMessageChunksCount; i++)
            {
                _totalTime += RunLoadLoops();
            }
        }

        private double RunLoadLoops()
        {
            _receivedCount = 0;
            _loopTime = DateTime.Now.TimeOfDay;

            for (var i = 0; i < SendMessagesCount; i++)
            {
                Messenger.Publish(_payload);
            }

            _loopTime = DateTime.Now.TimeOfDay - _loopTime;
            return _loopTime.TotalSeconds;
        }

        private void OnTestCallback(LoadTestPayload payload)
        {
            Assert.That(payload, Is.Not.Null);
            _receivedCount++;

            if(SendMessagesCount != _receivedCount)
            {
                return;
            }

            Debug.LogFormat("{0}: sent {1} messages, received {2} messages, took {3} seconds"
                , nameof(MessengerLoadMultithreadingTest)
                , SendMessagesCount
                , _receivedCount
                , Math.Round(_loopTime.TotalSeconds, 3));
            Assert.That(_receivedCount, Is.EqualTo(SendMessagesCount));
            Assert.That(SendMessageChunksCount, Is.GreaterThan(0));

            if (_receivedCount != SendMessageChunksCount * SendMessageChunksCount)
            {
                return;
            }
            var avgTime = Math.Round(_totalTime/SendMessageChunksCount, 3);
            Debug.LogFormat("{0}: average time {1}"
                , nameof(MessengerLoadMultithreadingTest)
                , avgTime);
        }
    }
}

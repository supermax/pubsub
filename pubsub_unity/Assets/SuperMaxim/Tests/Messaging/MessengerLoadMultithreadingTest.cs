using System;
using System.Collections;
using System.Threading;
using NUnit.Framework;
using SuperMaxim.Logging;
using SuperMaxim.Messaging;
using UnityEngine;
using UnityEngine.TestTools;

namespace SuperMaxim.Tests.Messaging
{
    public class MessengerLoadMultithreadingTest
    {
        private const int SendMessageChunksCount = 10;

        private const int SendMessagesCount = 10;

        private int _receivedCount;

        private double _totalTime;

        private TimeSpan _loopTime;

        private Thread _testThread;

        private readonly LoadTestPayload _payload = new LoadTestPayload();

        private class LoadTestPayload
        {

        }

        [UnityTest]
        public IEnumerator TestLoadAsync()
        {
            Messenger.Default.Subscribe<LoadTestPayload>(OnTestCallback);

            _testThread = new Thread(TestLoad);
            _testThread.Start();

            var wait = new WaitForSeconds(1f);
            while (_testThread.IsAlive)
            {
                yield return wait;
            }

            yield return null;
        }

        private void TestLoad()
        {
            _totalTime = 0.0;
            for (var i = 0; i < SendMessageChunksCount; i++)
            {
                _totalTime += TestLoadLoop();
            }
        }

        private double TestLoadLoop()
        {
            _receivedCount = 0;
            _loopTime = DateTime.Now.TimeOfDay;

            for (var i = 0; i < SendMessagesCount; i++)
            {
                Messenger.Default.Publish(_payload);
            }

            _loopTime = DateTime.Now.TimeOfDay - _loopTime;
            return _loopTime.TotalSeconds;
        }

        private void OnTestCallback(LoadTestPayload payload)
        {
            _receivedCount++;

            if(SendMessagesCount != _receivedCount)
            {
                return;
            }

            Loggers.Console.LogInfo("Multithreading Load Test: sent {0} messages, received {1} messages, took {2} seconds",
                                SendMessagesCount, _receivedCount, Math.Round(_loopTime.TotalSeconds, 3));

            Assert.AreEqual(SendMessagesCount, _receivedCount);

            if (_receivedCount != SendMessageChunksCount * SendMessageChunksCount)
            {
                return;
            }
            Loggers.Console.LogInfo("Multithreading Load Test: average time {0}",
                                Math.Round(_totalTime / SendMessageChunksCount, 3));
        }
    }
}

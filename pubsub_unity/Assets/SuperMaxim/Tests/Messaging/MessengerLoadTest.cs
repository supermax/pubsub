using System;
using NUnit.Framework;
using SuperMaxim.Logging;
using SuperMaxim.Messaging;
using UnityEngine;

namespace SuperMaxim.Tests.Messaging
{
    public class MessengerLoadTest
    {
        private const int SendMessageChunksCount = 10;

        private const int SendMessagesCount = 100000;

        private int _receivedCount;

        private readonly LoadTestPayload _payload = new LoadTestPayload();

        private class LoadTestPayload
        {
            
        }

        [Test]
        public void TestLoad()
        {
            Messenger.Default.Subscribe<LoadTestPayload>(OnTestCallback);
            
            var time = 0.0;
            for (var i = 0; i < SendMessageChunksCount; i++)
            {
                time += TestLoadLoop();
            }
            
            Loggers.Console.LogInfo("Load Test: average time {0}", Math.Round(time / SendMessageChunksCount, 3));
        }

        private double TestLoadLoop()
        {
            Loggers.Console.Config.IsEnabled = false;
            
            _receivedCount = 0;
            var time = DateTime.Now.TimeOfDay;

            for (var i = 0; i < SendMessagesCount; i++)
            {
                Messenger.Default.Publish(_payload);
            }
            
            time = DateTime.Now.TimeOfDay - time;
            
            Loggers.Console.Config.IsEnabled = true;
            Loggers.Console.LogInfo("Load Test: sent {0} messages, received {1} messages, took {2} seconds",
                                SendMessagesCount, _receivedCount, Math.Round(time.TotalSeconds, 3));

            Assert.AreEqual(SendMessagesCount, _receivedCount);

            return time.TotalSeconds;
        }

        private void OnTestCallback(LoadTestPayload payload)
        {
            _receivedCount++;
        }
    }
}

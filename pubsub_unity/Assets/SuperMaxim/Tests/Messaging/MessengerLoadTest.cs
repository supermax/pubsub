using System;
using System.Diagnostics;
using NUnit.Framework;
using SuperMaxim.Tests.Messaging.Fixtures;

namespace SuperMaxim.Tests.Messaging
{
    [TestFixture]
    public class MessengerLoadTest : BaseMessengerTest
    {
        private readonly LoadTestPayload _payload = new();

        [Test]
        [TestCase(10, 100000
            , Category = "Load Test"
            , Description = "Publishing thousands of payloads in loop")]
        public void TestLoad(int sendMessageChunksCount, int sendMessagesCount)
        {
            Assert.That(sendMessageChunksCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(sendMessagesCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(Messenger, Is.Not.Null);

            Messenger.Subscribe<LoadTestPayload>(OnTestCallback);

            var time = 0.0;
            for (var chunk = 0; chunk < sendMessageChunksCount; chunk++)
            {
                time += LoadLoop(sendMessagesCount);
            }

            Debug.WriteLine("{0}: average time {1}"
                , nameof(MessengerLoadTest)
                , Math.Round(time / sendMessageChunksCount, 3));
            Assert.That(time, Is.GreaterThan(0.0));
            Assert.That(time, Is.LessThan(5.0));
        }

        private double LoadLoop(int sendMessagesCount)
        {
            Logger.Config.IsEnabled = false;

            _payload.ReceivedCount = 0;
            var time = DateTime.Now.TimeOfDay;

            for (var msgNum = 0; msgNum < sendMessagesCount; msgNum++)
            {
                Messenger.Publish(_payload);
            }

            time = DateTime.Now.TimeOfDay - time;

            Logger.Config.IsEnabled = true;

            Debug.WriteLine("{0}: sent {1} messages, received {2} messages, took {3} seconds"
                , nameof(MessengerLoadTest)
                , sendMessagesCount
                , _payload.ReceivedCount
                , Math.Round(time.TotalSeconds, 3));

            Assert.That(_payload.ReceivedCount, Is.EqualTo(sendMessagesCount));

            return time.TotalSeconds;
        }

        private static void OnTestCallback(LoadTestPayload payload)
        {
            payload.ReceivedCount++;
        }
    }
}

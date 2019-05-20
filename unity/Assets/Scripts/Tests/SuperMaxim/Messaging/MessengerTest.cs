using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using SuperMaxim.Messaging;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class MessengerTest
    {
        [Test]
        public void MessengerTestSimplePasses()
        {
            Messenger.Default.Subscribe<string>(OnStringCallback);
        }

        [UnityTest]
        public IEnumerator MessengerTestWithEnumeratorPasses()
        {
            yield return null;

            Messenger.Default.Publish("Hello World! [1]");

            yield return null;

            Messenger.Default.Publish("Hello World! [2]");
        }

        private static void OnStringCallbackStatic(string str)
        {
            Debug.LogFormat("[OnStringCallbackStatic] String Payload: {0}", str);

            Messenger.Default.Unsubscribe<string>(OnStringCallbackStatic);
        }

        private void OnStringCallback(string str)
        {
            Debug.LogFormat("[OnStringCallback] String Payload: {0}", str);

            Messenger.Default.Unsubscribe<string>(OnStringCallback);
            Messenger.Default.Subscribe<string>(OnStringCallbackStatic);
        }

        [Test]
        public void MessengerTestPublishFromNewThread()
        {
            Messenger.Default.Subscribe<int>(OnPublishFromNewThreadCallback);

            Debug.LogFormat("[MessengerTestPublishFromNewThread] Thread ID: {0}", 
                                Thread.CurrentThread.ManagedThreadId);

            var th = new Thread(PublishFromNewThread);
            th.Start();
        }

        private void PublishFromNewThread()
        {
            Debug.LogFormat("[PublishFromNewThread] Thread ID: {0}", 
                                Thread.CurrentThread.ManagedThreadId);

            Messenger.Default.Publish(Thread.CurrentThread.ManagedThreadId);
        }

        private void OnPublishFromNewThreadCallback(int number)
        {
            Debug.LogFormat("[OnPublishFromNewThreadCallback] Int Payload: {0} (Thread ID: {1})", 
                                number, Thread.CurrentThread.ManagedThreadId);

            Assert.AreNotEqual(number, Thread.CurrentThread.ManagedThreadId);
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using SuperMaxim.Messaging;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class MessengerTest
    {
        // A Test behaves as an ordinary method
        [Test]
        public void MessengerTestSimplePasses()
        {
            Messenger.Default.Subscribe<string>(OnMsg);
        }

        private static void OnMsgStatic(string str)
        {

        }

        private void OnMsg(string str)
        {
            Debug.LogFormat("Msg: {0}", str);

            Messenger.Default.Unsubscribe<string>(OnMsg);
        }

        [UnityTest]
        public IEnumerator MessengerTestWithEnumeratorPasses()
        {
            yield return null;

            Messenger.Default.Publish("Hello World! [1]");

            yield return null;

            Messenger.Default.Publish("Hello World! [2]");
        }
    }
}

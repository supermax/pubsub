using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using SuperMaxim.Core.WeakRef;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class WeakRefDelegateTest
    {
        [OneTimeSetUp]
        public void Init()
        {
            // TODO
        }

        [UnityTest]
        public IEnumerator TestVoidCallbackWithEnumerator()
        {
            var test = new Test();
            var wr = new WeakRefDelegate(test.TestVoidCallbackDelegate);

            var res = wr.Invoke(null);

            Debug.LogFormat("[{0}] {1}: result: {2}, isAlive: {3}",
                wr.GetHashCode(), test.TestVoidCallbackName, res, wr.IsAlive);

#pragma warning disable IDE0059 // Value assigned to symbol is never used
            test.TestVoidCallbackDelegate = null;
            test = null;
            yield return null;
#pragma warning restore IDE0059 // Value assigned to symbol is never used

            var time = Time.fixedTime;
            var wait = new WaitForSeconds(1f);

            while (wr.IsAlive && Time.fixedTime - time < 10)
            {
                yield return wait;
            }

            if(wr.IsAlive)
            {
                Debug.LogErrorFormat("[{0}] isAlive: {1}, time (s): {2:N}", 
                    wr.GetHashCode(), wr.IsAlive, Time.fixedTime - time);
            }
            else
            {
                Debug.LogFormat("[{0}] isAlive: {1}, time (s): {2:N}", 
                    wr.GetHashCode(), wr.IsAlive, Time.fixedTime - time);
            }
        }

        private class Test
        {
            public Action TestVoidCallbackDelegate { get; set; }
            public string TestVoidCallbackName { get; set; }

            public Test()
            {
                TestVoidCallbackDelegate = TestVoidCallback;
                TestVoidCallbackName = TestVoidCallbackDelegate.Method.Name;
            }

            public void TestVoidCallback()
            {
                Debug.LogFormat("{0} invoked", TestVoidCallbackName);
            }
        }
    }
}

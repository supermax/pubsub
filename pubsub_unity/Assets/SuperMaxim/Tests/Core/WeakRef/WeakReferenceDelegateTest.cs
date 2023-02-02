using System;
using System.Diagnostics;
using NUnit.Framework;
using SuperMaxim.Core.WeakRef;

namespace SuperMaxim.Tests.Core.WeakRef
{
    [TestFixture]
    public class WeakReferenceDelegateTest
    {
        [TestCase]
        public void TestReferenceDisposal()
        {
            var testTarget = new WeakRefTestTarget();
            WeakReferenceDelegate weakRef;
            using (weakRef = new WeakReferenceDelegate((Action)testTarget.TestVoidCallback))
            {
                Debug.WriteLine("[{0}] Init: {1}", nameof(TestReferenceDisposal), weakRef);
                Assert.That(weakRef, Is.Not.Null);
                Assert.That(weakRef.IsAlive, Is.True);
                Assert.That(weakRef.Target, Is.Not.Null);
                var res = weakRef.Invoke(null);

                Debug.WriteLine("[{0}] {1}: result: {2}, isAlive: {3}",
                    weakRef.GetHashCode()
                    , nameof(testTarget.TestVoidCallback)
                    , res
                    , weakRef.IsAlive);
            }

            Assert.That(weakRef.IsAlive, Is.False);
            Assert.That(weakRef.Target, Is.Null);
            Debug.WriteLine("[{0}] isAlive: {1}", weakRef.GetHashCode(), weakRef.IsAlive);
        }

        [TestCase]
        public void TestInvokeDisposedInstance()
        {
            var testTarget = new WeakRefTestTarget();
            WeakReferenceDelegate weakRef;
            using (weakRef = new WeakReferenceDelegate((Action)testTarget.TestVoidCallback))
            {
                Debug.WriteLine("[{0}] Init: {1}", nameof(TestInvokeDisposedInstance), weakRef);
                Assert.That(weakRef, Is.Not.Null);
                Assert.That(weakRef.IsAlive, Is.True);
            }

            Assert.That(weakRef.IsAlive, Is.False);
            Debug.WriteLine("[{0}] isAlive: {1}", weakRef.GetHashCode(), weakRef.IsAlive);

            var res = weakRef.Invoke(null);

            Debug.WriteLine("[{0}] {1}: result: {2}, isAlive: {3}",
                weakRef.GetHashCode()
                , nameof(testTarget.TestVoidCallback)
                , res
                , weakRef.IsAlive);
            Assert.That(res, Is.Null);
        }

        [TestCase]
        public void TestEquals()
        {
            var testTarget = new WeakRefTestTarget();
            var callback = (Action)testTarget.TestVoidCallback;
            using var weakRef = new WeakReferenceDelegate(callback);
            Debug.WriteLine("[{0}] Init: {1}", nameof(TestEquals), weakRef);

            var testWeakRef = new WeakReferenceDelegate(callback);
            Assert.That(weakRef, Is.Not.Null);
            Assert.That(weakRef.IsAlive, Is.True);
            Assert.That(weakRef, Is.Not.EqualTo(null));
            Assert.That(weakRef, Is.EqualTo(testWeakRef));
            Assert.That(weakRef, Is.Not.GreaterThan(testWeakRef));
            Assert.That(weakRef, Is.GreaterThanOrEqualTo(testWeakRef));

            using var weakRef2 = new WeakReferenceDelegate(callback);
            Assert.That(weakRef, Is.EqualTo(weakRef2));
            Assert.That(weakRef.CompareTo(weakRef2), Is.Zero);
            Assert.That(weakRef2, Is.EqualTo(testWeakRef));

            var equals = weakRef.Equals(weakRef2);
            Assert.That(equals, Is.True);

            equals = weakRef.Equals(null);
            Assert.That(equals, Is.False);

            // ReSharper disable once SuspiciousTypeConversion.Global >> Needed for Unit Test Scenario
            equals = weakRef.Equals(testTarget);
            Assert.That(equals, Is.False);
        }

        [TestCase]
        public void TestCompare()
        {
            var testTarget = new WeakRefTestTarget();
            var action = (Action)testTarget.TestVoidCallback;
            using var weakRef = new WeakReferenceDelegate(action);
            Debug.WriteLine("[{0}] Init: {1}", nameof(TestCompare), weakRef);

            Assert.That(weakRef, Is.Not.Null);
            Assert.That(weakRef.IsAlive, Is.True);
            Assert.That(weakRef.CompareTo(null), Is.EqualTo(-1));

            var leftGreaterThanRight = weakRef > null;
            Assert.That(leftGreaterThanRight, Is.True);

            var leftLessThanRight = null < weakRef;
            Assert.That(leftLessThanRight, Is.True);

            var leftIsLessAndEqualsToRight = weakRef <= null;
            Assert.That(leftIsLessAndEqualsToRight, Is.False);

            var leftIsGreaterAndEqualsToRight = weakRef >= null;
            Assert.That(leftIsGreaterAndEqualsToRight, Is.True);
        }
    }
}

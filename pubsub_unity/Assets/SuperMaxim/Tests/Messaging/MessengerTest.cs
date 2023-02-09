using System;
using System.Diagnostics;
using NUnit.Framework;
using SuperMaxim.Messaging.API;
using SuperMaxim.Tests.Messaging.Fixtures;

namespace SuperMaxim.Tests.Messaging
{
    [TestFixture]
    public class MessengerTest : BaseMessengerTest
    {
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var instance1 = Messenger.Subscribe<FilteredPayload>(FilterPayloadCondition);
            Assert.That(instance1, Is.Not.Null);
            Assert.That(Messenger, Is.SameAs(instance1));

            var instance2 = Messenger.Subscribe<FilteredPayload>(OnFilteredPayloadCallback);
            Assert.That(instance2, Is.Not.Null);
            Assert.That(instance2, Is.SameAs(instance1));
        }

        private static bool FilterPayloadCondition(FilteredPayload payload)
        {
            Assert.That(payload, Is.Not.Null);

            Debug.WriteLine($"{nameof(payload.IsFilterOn)}: {payload.IsFilterOn}");
            return !payload.IsFilterOn;
        }

        [Test]
        public void TestPublishFilteredPayload()
        {
            Assert.That(Messenger, Is.Not.Null);

            var payload = new FilteredPayload {IsFilterOn = true};
            var instance1 = Messenger.Publish(payload);
            Assert.That(instance1, Is.Not.Null);
            Assert.That(Messenger, Is.SameAs(instance1));
            Assert.That(payload.CallbackCount, Is.Zero);

            payload.IsFilterOn = false;
            var instance2 = Messenger.Publish(payload);
            Assert.That(instance2, Is.Not.Null);
            Assert.That(instance2, Is.SameAs(instance1));
            Assert.That(payload.CallbackCount, Is.EqualTo(1));
        }

        // ReSharper disable once MemberCanBeMadeStatic.Local (SHOULD NOT BE STATIC!)
        private void OnFilteredPayloadCallback(FilteredPayload payload)
        {
            payload.CallbackCount++;
            Assert.That(payload, Is.Not.Null);
            Debug.WriteLine($"{nameof(payload)}.{nameof(payload.IsFilterOn)}:{payload.IsFilterOn}");

            Assert.That(payload, Is.Not.Null);
            Assert.That(!payload.IsFilterOn, Is.True);
        }

        [Test]
        public void TestSubscribeAndPublish()
        {
            Assert.That(Messenger, Is.Not.Null);

            var instance = Messenger.Subscribe<MessengerTestPayload<IMessenger>>(OnCallback);
            Assert.That(instance, Is.Not.Null);
            Assert.That(Messenger, Is.SameAs(instance));

            var payload = new MessengerTestPayload<IMessenger>
                {
                    Id = UnityEngine.Random.Range(-1, 1)
                    , Data = Messenger
                };
            var instance1 = Messenger.Publish(payload);
            Assert.That(instance1, Is.Not.Null);
            Assert.That(instance1, Is.SameAs(instance));
            Assert.That(payload.CallbackCount, Is.EqualTo(1));
        }

        [Test]
        public void TestSubscribeAndPublishStatic()
        {
            Assert.That(Messenger, Is.Not.Null);

            var instance = Messenger.Subscribe<MessengerTestPayload<IMessenger>>(OnStaticCallback);
            Assert.That(instance, Is.Not.Null);
            Assert.That(Messenger, Is.SameAs(instance));

            var payload = new MessengerTestPayload<IMessenger>
                {
                    Id = UnityEngine.Random.Range(-1, 1)
                    , Data = Messenger
                };
            var instance1 = Messenger.Publish(payload);
            Assert.That(instance1, Is.Not.Null);
            Assert.That(instance1, Is.SameAs(instance));
            Assert.That(payload.CallbackCount, Is.EqualTo(1));
        }

        [Test]
        public void TestSubscribeAndPublishToObjectWithPredicate()
        {
            Assert.That(Messenger, Is.Not.Null);

            var instance1 = Messenger.Subscribe<MessengerTestPayload<int>>(OnSubscribeToObjectWithPredicate, ObjectPredicate);
            Assert.That(instance1, Is.Not.Null);
            Assert.That(Messenger, Is.SameAs(instance1));

            var instance2 = Messenger.Publish(
                new MessengerTestPayload<int>
                    {
                        Id = UnityEngine.Random.Range(-1, 1)
                    });
            Assert.That(instance2, Is.Not.Null);
            Assert.That(instance2, Is.SameAs(instance1));
        }

        private static bool ObjectPredicate(MessengerTestPayload<int> payload)
        {
            Assert.That(payload, Is.Not.Null);

            var accepted = payload.Id % 2 == 0;
            Debug.WriteLine("[{0}] Object Payload Id: {1}, Accepted: {2}", nameof(ObjectPredicate), payload.Id, accepted);
            return accepted;
        }

        private static void OnSubscribeToObjectWithPredicate(MessengerTestPayload<int> payload)
        {
            payload.CallbackCount++;
            Assert.That(payload, Is.Not.Null);
            Debug.WriteLine("[{0}] Object Payload Id: {1}", nameof(OnSubscribeToObjectWithPredicate), payload.Id);
        }

        private static void OnStaticCallback(MessengerTestPayload<IMessenger> payload)
        {
            payload.CallbackCount++;
            Assert.That(payload, Is.Not.Null);
            Debug.WriteLine("[{0}] Payload: {1}", nameof(OnStaticCallback), payload);

            payload.Data?.Unsubscribe<MessengerTestPayload<IMessenger>>(OnStaticCallback);
        }

        // ReSharper disable once MemberCanBeMadeStatic.Local (MUST NOT BE STATIC!)
        private void OnCallback(MessengerTestPayload<IMessenger> payload)
        {
            payload.CallbackCount++;
            Assert.That(payload, Is.Not.Null);
            Debug.WriteLine("[{0}] String Payload: {1}", nameof(OnCallback), payload);

            payload.Data?.Unsubscribe<MessengerTestPayload<IMessenger>>(OnCallback);
            payload.Data?.Subscribe<MessengerTestPayload<IMessenger>>(OnStaticCallback);
        }

        [Test]
        public void TestEmptySubscribersCase()
        {
            var payload = new MessengerTestPayload<bool>();
            var messenger = Messenger.Publish(payload);
            Assert.That(messenger, Is.Not.Null);
            Assert.That(payload.CallbackCount, Is.EqualTo(0));

            var messenger1 = Messenger.Subscribe<MessengerTestPayload<bool>>(OnMessengerCallback);
            Assert.That(messenger1, Is.Not.Null);

            var messenger2 = Messenger.Publish(payload);
            Assert.That(messenger2, Is.Not.Null);
            Assert.That(messenger1, Is.SameAs(messenger2));
            Assert.That(payload.CallbackCount, Is.EqualTo(1));

            var messenger3 = Messenger.Unsubscribe<MessengerTestPayload<bool>>(OnMessengerCallback);
            Assert.That(messenger3, Is.Not.Null);
            Assert.That(messenger2, Is.SameAs(messenger3));

            var messenger4 = Messenger.Publish(payload);
            Assert.That(messenger4, Is.Not.Null);
            Assert.That(messenger3, Is.SameAs(messenger4));
            Assert.That(payload.CallbackCount, Is.EqualTo(1));
        }

        [Test]
        public void TestSameSubscriber()
        {
            var messenger1 = Messenger.Subscribe<MessengerTestPayload<bool>>(OnMessengerCallback);
            Assert.That(messenger1, Is.Not.Null);

            var messenger2 = Messenger.Subscribe<MessengerTestPayload<bool>>(OnMessengerCallback);
            Assert.That(messenger2, Is.Not.Null);
            Assert.That(messenger1, Is.SameAs(messenger2));

            var payload = new MessengerTestPayload<bool>();
            var messenger3 = Messenger.Publish(payload);
            Assert.That(messenger3, Is.Not.Null);
            Assert.That(messenger2, Is.SameAs(messenger3));
            Assert.That(payload.CallbackCount, Is.EqualTo(1));
        }

        [Test]
        public void TestUnsubscribeAction()
        {
            var messenger1 = Messenger.Unsubscribe<MessengerTestPayload<bool>>(OnMessengerCallback);
            Assert.That(messenger1, Is.Not.Null);

            var messenger2 = messenger1.Unsubscribe<FilteredPayload>(OnFilteredPayloadCallback);
            Assert.That(messenger2, Is.Not.Null);
        }

        private static void OnMessengerCallback(MessengerTestPayload<bool> payload)
        {
            payload.CallbackCount++;
            Assert.That(payload, Is.Not.Null);
        }

        private static Action<MessengerTestPayload<int>> GetCallback()
        {
            return null!;
        }

        private static Predicate<MessengerTestPayload<int>> GetPredicate()
        {
            return null!;
        }

        private static MessengerTestPayload<int> GetPayload()
        {
            return null!;
        }

        [Test]
        public void TestPublishSubScribeUnsubscribeWithNullPayload()
        {
            Assert.Throws<ArgumentNullException>(() => Messenger.Subscribe(GetPredicate()));
            Assert.Throws<ArgumentNullException>(() => Messenger.Unsubscribe(GetPredicate()));

            Assert.Throws<ArgumentNullException>(() => Messenger.Subscribe(GetCallback()));
            Assert.Throws<ArgumentNullException>(() => Messenger.Unsubscribe(GetCallback()));

            Assert.Throws<ArgumentNullException>(() => Messenger.Publish(GetPayload()));
        }

        [Test]
        public void TestPubSubWithStateObject()
        {
            var state = new Reference<string>{ Ref = nameof(Reference<string>) };
            var instance1 = Messenger
                .Subscribe<MessengerTestPayload<int>
                    , Reference<string>>(OnCallbackWithStateObject, OnPredicateWithStateObject, state);
            Assert.That(instance1, Is.Not.Null);
            Assert.That(Messenger, Is.SameAs(instance1));
            Assert.That(state.Ref, Is.Not.Null);
            Assert.That(state.Ref, Is.SameAs(nameof(Reference<string>)));

            var instance2 = Messenger
                .Subscribe<MessengerTestPayload<int>
                    , Reference<string>>(OnTypePredicateWithStateObject, state);
            Assert.That(instance2, Is.Not.Null);
            Assert.That(instance2, Is.SameAs(instance1));

            var payload = new MessengerTestPayload<int>();
            Messenger.Publish(payload);
            Assert.That(payload.CallbackCount, Is.GreaterThan(0));
            Assert.That(state.Ref, Is.Not.Null);
            Assert.That(state.Ref, Is.SameAs(nameof(OnCallbackWithStateObject)));

            UnityEngine.Debug.LogFormat("{0}: {1}, {2}: {3}"
                , nameof(payload.CallbackCount)
                , payload.CallbackCount
                , nameof(state.Ref)
                , state.Ref);
        }

        private static bool OnTypePredicateWithStateObject(MessengerTestPayload<int> payload, Reference<string> stateObj)
        {
            Assert.That(payload, Is.Not.Null);
            Assert.That(stateObj, Is.Not.Null);

            stateObj.Ref = nameof(OnTypePredicateWithStateObject);
            return true;
        }

        private static bool OnPredicateWithStateObject(MessengerTestPayload<int> payload, Reference<string> stateObj)
        {
            Assert.That(payload, Is.Not.Null);
            Assert.That(stateObj, Is.Not.Null);
            Assert.That(stateObj.Ref, Is.SameAs(nameof(OnTypePredicateWithStateObject)));

            var accepted = stateObj.Ref == nameof(OnTypePredicateWithStateObject);
            return accepted;
        }

        private void OnCallbackWithStateObject(MessengerTestPayload<int> payload, Reference<string> stateObj)
        {
            payload.CallbackCount++;
            stateObj.Ref = nameof(OnCallbackWithStateObject);
        }

        [Test]
        public void TestSubscribeValidations()
        {
            Assert.That(Messenger, Is.Not.Null);

            Assert.Throws<ArgumentNullException>(() => Messenger.Subscribe<MessengerTestPayload<int>>(null!));
            Assert.Throws<ArgumentNullException>(() => Messenger.Subscribe<MessengerTestPayload<int>>(null!, null));

            Action<MessengerTestPayload<int>> callback1 = null;
            // ReSharper disable once ExpressionIsAlwaysNull
            Assert.Throws<ArgumentNullException>(() => Messenger.Subscribe(callback1));

            Action<MessengerTestPayload<int>, object> callback2 = null;
            // ReSharper disable once ExpressionIsAlwaysNull
            Assert.Throws<ArgumentNullException>(() => Messenger.Subscribe(callback2));

            Func<MessengerTestPayload<int>, object, bool> predicate = null;
            // ReSharper disable once ExpressionIsAlwaysNull
            Assert.Throws<ArgumentNullException>(() => Messenger.Subscribe(predicate));
        }

        [Test]
        public void TestUnsubscribeValidations()
        {
            Assert.That(Messenger, Is.Not.Null);

            Assert.Throws<ArgumentNullException>(() => Messenger.Unsubscribe<MessengerTestPayload<int>>(null!));

            Action<MessengerTestPayload<int>> callback1 = null;
            // ReSharper disable once ExpressionIsAlwaysNull
            Assert.Throws<ArgumentNullException>(() => Messenger.Unsubscribe(callback1));

            Action<MessengerTestPayload<int>, object> callback2 = null;
            // ReSharper disable once ExpressionIsAlwaysNull
            Assert.Throws<ArgumentNullException>(() => Messenger.Unsubscribe(callback2));

            Func<MessengerTestPayload<int>, object, bool> predicate = null;
            // ReSharper disable once ExpressionIsAlwaysNull
            Assert.Throws<ArgumentNullException>(() => Messenger.Unsubscribe(predicate));
        }

        [Test]
        public void TestPublishValidations()
        {
            Assert.That(Messenger, Is.Not.Null);

            Assert.Throws<ArgumentNullException>(() => Messenger.Publish<MessengerTestPayload<int>>(null!));
            Assert.Throws<ArgumentNullException>(() => Messenger.Publish(null!, null!));
            Assert.Throws<ArgumentNullException>(() => Messenger.Publish(typeof(MessengerTestPayload<int>), null!));
            Assert.Throws<ArgumentNullException>(() => Messenger.Publish(null!, new MessengerTestPayload<int>()));
            Assert.Throws<InvalidCastException>(() => Messenger.Publish(typeof(MessengerTestPayload<double>), new MessengerTestPayload<int>()));
        }

        [Test]
        public void TestMethodsDuringPublishing()
        {
            Assert.That(Messenger, Is.Not.Null);

            void Callback2(MessengerTestPayload<double> payload)
            {
                payload.CallbackCount++;

                Messenger.Unsubscribe((Action<MessengerTestPayload<double>>)Callback2);
            }

            void Callback1(MessengerTestPayload<int> payload)
            {
                payload.CallbackCount++;

                Messenger.Subscribe((Action<MessengerTestPayload<double>>)Callback2);
                Messenger.Publish(new MessengerTestPayload<double>());
                Messenger.Unsubscribe((Action<MessengerTestPayload<int>>)Callback1);
            }

            Messenger.Subscribe((Action<MessengerTestPayload<int>>)Callback1);
            Messenger.Publish(new MessengerTestPayload<int>());
        }

        [Test]
        public void TestMethodsWithStateDuringPublishing()
        {
            Assert.That(Messenger, Is.Not.Null);

            bool Predicate(MessengerTestPayload<int> payload, object state)
            {
                payload.CallbackCount++;

                return true;
            }

            void Callback2(MessengerTestPayload<double> payload, object state)
            {
                payload.CallbackCount++;

                Messenger.Unsubscribe((Action<MessengerTestPayload<double>, object>)Callback2);
            }

            void Callback1(MessengerTestPayload<int> payload, object state)
            {
                payload.CallbackCount++;

                Messenger.Subscribe((Action<MessengerTestPayload<double>, object>)Callback2, stateObj: this);
                Messenger.Publish(new MessengerTestPayload<double>());
                Messenger.Unsubscribe((Action<MessengerTestPayload<int>, object>)Callback1);
            }

            Messenger.Subscribe(Callback1, (Func<MessengerTestPayload<int>, object, bool>)Predicate, this);
            Messenger.Publish(new MessengerTestPayload<int>());
        }
    }
}

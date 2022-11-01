﻿using System;
using System.Reflection;
using SuperMaxim.Core.Logging;

namespace SuperMaxim.Messaging
{
    /// <summary>
    /// Pub-Sub Messenger Subscriber
    /// </summary>
    /// <remarks>
    /// A holder of weak delegate reference.
    /// Used in <see cref="Messenger"/> class.
    /// </remarks>
    internal class Subscriber : IDisposable
    {
        // reference to the owner of callback method
        private WeakReference _callbackTarget;
        // callback method info
        private MethodInfo _callbackMethod;

        // reference to the owner of predicate method
        private WeakReference _predicateTarget;
        // predicate method info
        private MethodInfo _predicateMethod;

        /// <summary>
        /// Indicates if callback owner is alive 
        /// </summary>
        public bool IsAlive
        {
            get
            {
                if(_callbackMethod == null)
                {
                    return false;
                }
                if(_callbackMethod.IsStatic)
                {
                    return true;
                }
                var isAlive = _callbackTarget is {IsAlive: true, Target: { }};
                return isAlive;
            }
        }

        /// <summary>
        /// The type of the payload
        /// </summary>
        public Type PayloadType
        {
            get;
            private set;
        }

        /// <summary>
        /// The ID if this instance
        /// </summary>
        public int Id
        {
            get;
            private set;
        }

        private ILogger _logger;

        public bool IsPredicate
        {
            get;
            private set;
        }

        public override int GetHashCode()
        {
            return Id;
        }

        /// <summary>
        /// Subscriber's Constructor
        /// </summary>
        /// <param name="payloadType">The type of the payload</param>
        /// <param name="predicate">The predicate delegate</param>
        /// <param name="logger">The logger to log info or errors</param>
        /// <exception cref="ArgumentNullException">
        /// The exception is thrown in case 'payloadType' or 'predicate' null
        /// </exception>
        internal Subscriber(Type payloadType, Delegate predicate, ILogger logger)
        {
            // validate params
            if(payloadType == null)
            {
                throw new ArgumentNullException(nameof(payloadType));
            }
            if(predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            
            IsPredicate = true;
            Init(payloadType, predicate, null, logger);
        }

        /// <summary>
        /// Subscriber's Constructor
        /// </summary>
        /// <param name="payloadType">The type of the payload</param>
        /// <param name="callback">The callback delegate</param>
        /// <param name="predicate">The predicate delegate</param>
        /// /// <param name="logger">The logger to log info or errors</param>
        /// <exception cref="ArgumentNullException">
        /// The exception is thrown in case 'payloadType' or 'callback' null
        /// </exception>
        internal Subscriber(Type payloadType, Delegate callback, Delegate predicate, ILogger logger)
        {
            // validate params
            if(payloadType == null)
            {
                throw new ArgumentNullException(nameof(payloadType));
            }
            if(callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }
            
            Init(payloadType, callback, predicate, logger);
        }

        private void Init(Type payloadType, Delegate callback, Delegate predicate, ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // assign values to vars
            PayloadType = payloadType;
            Id = callback.GetHashCode();
            _callbackMethod = callback.Method;

            // check if callback method is not a static method
            if (!_callbackMethod.IsStatic &&
                callback.Target != null)
            {
                // init weak reference to callback owner
                _callbackTarget = new WeakReference(callback.Target);
            }

            // --- init predicate ---
            if (predicate == null)
            {
                return;
            }
            _predicateMethod = predicate.Method;

            if (!_predicateMethod.IsStatic &&
                !Equals(predicate.Target, callback.Target))
            {
                _predicateTarget = new WeakReference(predicate.Target);
            }
        }

        /// <summary>
        /// Invokes callback method with given payload instance
        /// </summary>
        /// <param name="payload"></param>
        /// <typeparam name="T"></typeparam>
        public object Invoke<T>(T payload)
        {
            // validate callback method info
            if(_callbackMethod == null)
            {
                _logger.LogError($"{nameof(_callbackMethod)} is null.");
                return null;
            }
            if(!_callbackMethod.IsStatic && 
                _callbackTarget is not {IsAlive: true})
            {
                _logger.LogWarning($"{nameof(_callbackMethod)} is not alive.");
                return null;
            }

            // check predicate state
            if (!InvokePredicate(payload))
            {
                return null;
            }

            // invoke callback method
            object callbackTarget = null;
            if(!_callbackMethod.IsStatic && 
               _callbackTarget is {IsAlive: true})
            {
                callbackTarget = _callbackTarget.Target;
            }
            var res = _callbackMethod.Invoke(callbackTarget, new object[] {payload});
            return res;
        }

        private bool InvokePredicate<T>(T payload)
        {
            if (_predicateMethod == null)
            {
                return true;
            }
            
            // get reference to the predicate function owner
            object predicateTarget = null;
            if (!_predicateMethod.IsStatic)
            {
                if (_predicateTarget is {IsAlive: true})
                {
                    predicateTarget = _predicateTarget.Target;
                }
                else if (_callbackTarget is {IsAlive: true})
                {
                    predicateTarget = _callbackTarget.Target;
                }
            }

            // check if predicate returned 'true'
            var isAccepted = (bool)_predicateMethod.Invoke(predicateTarget, new object[] {payload});
            return isAccepted;
        }

        /// <summary>
        /// Dispose this instance
        /// </summary>
        public void Dispose()
        {
            _logger = null;
            _callbackMethod = null;
            
            if(_callbackTarget != null)
            {             
                _callbackTarget.Target = null;
                _callbackTarget = null;
            }
            
            _predicateMethod = null;
            if (_predicateTarget == null)
            {
                return;
            }
            _predicateTarget.Target = null;
            _predicateTarget = null;
        }
    }
}
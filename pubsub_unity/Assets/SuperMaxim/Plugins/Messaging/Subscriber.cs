using System;
using System.Reflection;
using SuperMaxim.Logging;
using UnityEngine;

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
                if(_callbackTarget == null ||
                    !_callbackTarget.IsAlive ||
                    _callbackTarget.Target == null)
                {
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// The type of the payload
        /// </summary>
        public Type PayloadType
        {
            get;
        }

        /// <summary>
        /// The ID if this instance
        /// </summary>
        public int Id 
        {
            get;
        }

        public override int GetHashCode()
        {
            return Id;
        }

        /// <summary>
        /// Subscriber's Constructor
        /// </summary>
        /// <param name="payloadType">The type of the payload</param>
        /// <param name="callback">The callback delegate</param>
        /// <param name="predicate">The predicate delegate</param>
        /// <exception cref="ArgumentNullException">
        /// The exception is thrown in case 'payloadType' or 'callback' null
        /// </exception>
        public Subscriber(Type payloadType, Delegate callback, Delegate predicate = null)
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
            
            // assign values to vars
            PayloadType = payloadType;
            Id = callback.GetHashCode();
            _callbackMethod = callback.Method;

            // check if callback method is not a static method
            if(!_callbackMethod.IsStatic && 
                callback.Target != null)
            {
                // init weak reference to callback owner
                _callbackTarget = new WeakReference(callback.Target);
            }
            
            // --- init predicate ---
            if(predicate == null)
            {
                return;
            }            
            _predicateMethod = predicate.Method;

            if(!_predicateMethod.IsStatic && 
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
        public void Invoke<T>(T payload)
        {
            // validate callback method info
            if(_callbackMethod == null)
            {
                Loggers.Console.LogError($"{nameof(_callbackMethod)} is null.");
                return;
            }
            if(!_callbackMethod.IsStatic && 
                (_callbackTarget == null || 
                !_callbackTarget.IsAlive))
            {
                Loggers.Console.LogWarning($"{nameof(_callbackMethod)} is not alive.");
                return;
            }

            // get reference to the predicate function owner
            if(_predicateMethod != null)
            {
                object predicateTarget = null;
                if(!_predicateMethod.IsStatic)
                {
                    if(_predicateTarget != null && 
                        _predicateTarget.IsAlive)
                    {
                        predicateTarget = _predicateTarget.Target;
                    }
                    else if(_callbackTarget != null && 
                            _callbackTarget.IsAlive)
                    {
                        predicateTarget = _callbackTarget.Target;
                    }
                }

                // check if predicate returned 'true'
                var isAccepted = (bool)_predicateMethod.Invoke(predicateTarget, new object[] {payload});
                if(!isAccepted)
                {
                    // TODO log ?
                    return;
                }
            }

            // invoke callback method
            object callbackTarget = null;
            if(!_callbackMethod.IsStatic && 
                _callbackTarget != null && _callbackTarget.IsAlive)
            {
                callbackTarget = _callbackTarget.Target;
            }
            _callbackMethod.Invoke(callbackTarget, new object[] {payload});
        }

        /// <summary>
        /// Dispose this instance
        /// </summary>
        public void Dispose()
        {
            _callbackMethod = null;
            if(_callbackTarget != null)
            {             
                _callbackTarget.Target = null;
                _callbackTarget = null;
            }
            
            _predicateMethod = null;
            if(_predicateTarget != null)
            {
                _predicateTarget.Target = null;
                _predicateTarget = null;
            }
            //PayloadType = null;
        }
    }
}
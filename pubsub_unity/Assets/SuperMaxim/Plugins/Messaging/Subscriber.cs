using System;
using System.Reflection;

namespace SuperMaxim.Messaging
{
    internal class Subscriber : IDisposable
    {
        private WeakReference _callbackTarget;
        private MethodInfo _callbackMethod;

        private WeakReference _predicateTarget;
        private MethodInfo _predicateMethod;

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

        public Type PayloadType
        {
            get;
            private set;
        }

        public int Id 
        {
            get;
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public Subscriber(Type payloadType, Delegate callback, Delegate predicate = null)
        {
            if(callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }
            
            PayloadType = payloadType;
            Id = callback.GetHashCode();
            _callbackMethod = callback.Method;

            if(!_callbackMethod.IsStatic && 
                callback.Target != null)
            {
                _callbackTarget = new WeakReference(callback.Target);
            }

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

        public void Invoke<T>(T payload)
        {
            if(_callbackMethod == null)
            {
                // TODO write to log
                return;
            }
            if(!_callbackMethod.IsStatic && 
                (_callbackTarget == null || 
                !_callbackTarget.IsAlive))
            {
                // TODO write to log
                return;
            }

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

                var isAccepted = (bool)_predicateMethod.Invoke(predicateTarget, new object[] {payload});
                if(!isAccepted)
                {
                    // TODO log
                    return;
                }
            }

            object callbackTarget = null;
            if(!_callbackMethod.IsStatic && 
                _callbackTarget != null && _callbackTarget.IsAlive)
            {
                callbackTarget = _callbackTarget.Target;
            }
            _callbackMethod.Invoke(callbackTarget, new object[] {payload});
        }

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
            PayloadType = null;
        }
    }
}
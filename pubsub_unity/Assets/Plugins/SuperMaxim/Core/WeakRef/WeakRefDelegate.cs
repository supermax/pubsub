using System;
using System.Reflection;
using SuperMaxim.Core.Extensions;

namespace SuperMaxim.Core.WeakRef
{
    public class WeakRefDelegate : WeakRefWrapper, IEquatable<Delegate>, IComparable
    {
        private MethodInfo _method;

        public int Id { get; protected set; }

        public MethodInfo Method 
        {
            get
            {
                return _method;
            }
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public WeakRefDelegate(Delegate method) : base(method.Target)
        {
            _method = method.Method;
            Id = method.GetHashCode();
        }

        public object Invoke(object[] args)
        {
            if (_isDisposed || !IsAlive)
            {
                return null;
            }
            
            var result = _method.Invoke(Target, args);
            return result;
        }

        public bool Contains(Delegate method)
        {
            if(method == null || !IsAlive)
            {
                return false;
            }
            if(!Equals(Target, method.Target) || !Equals(_method, method.Method))
            {
                return false;
            }
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                _method = null;
            }
        }

        public bool Equals(Delegate other)
        {
            if(other == null) return false;
            var otherId = other.GetHashCode();
            var equals = Id == otherId;
            return equals;
        }

        public int CompareTo(object obj)
        {
            if(obj == null) return -1;
            var @delegate = obj as Delegate;
            if(@delegate == null) return -1;
            if(Equals(@delegate)) return 0;
            return -1;
        }        
    }
}
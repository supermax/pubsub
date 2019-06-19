using System;
using System.Reflection;

namespace SuperMaxim.Core.WeakRef
{
    public class WeakRefDelegate : WeakRefWrapper, IEquatable<Delegate>, IComparable
    {
        public int Id { get; protected set; }

        public MethodInfo Method { get; private set; }

        public override int GetHashCode()
        {
            return Id;
        }

        public WeakRefDelegate(Delegate method) : base(method.Target)
        {
            Method = method.Method;
            Id = method.GetHashCode();
        }

        public object Invoke(object[] args)
        {
            if (_isDisposed || !IsAlive)
            {
                return null;
            }
            
            var result = Method.Invoke(Target, args);
            return result;
        }

        public bool Contains(Delegate method)
        {
            if(method == null || !IsAlive)
            {
                return false;
            }
            if(!Equals(Target, method.Target) || !Equals(Method, method.Method))
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
                Method = null;
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

        public override string ToString()
        {
            return string.Format("{0}: {1}, {2}, {3}",
                                    GetType().Name, Id, Method, IsAlive);
        }
    }
}
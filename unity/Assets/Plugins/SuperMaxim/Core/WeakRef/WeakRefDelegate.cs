using System;
using System.Reflection;
using SuperMaxim.Core.Extensions;

namespace SuperMaxim.Core.WeakRef
{
    public class WeakRefDelegate : WeakRefWrapper
    {
        private MethodInfo _method;

        public enum WeakRefDelegateInvokeResult
        {
            Success,
            NotAlive,
            Disposed
        }

        protected WeakRefDelegate(Delegate method) : base(method.Target)
        {
            _method = method.Method;
        }

        public static WeakRefDelegate Create(Delegate method)
        {
            var instance = new WeakRefDelegate(method);
            return instance;
        }

        public WeakRefDelegateInvokeResult Invoke()
        {
            var result = Invoke(null);
            return result;
        }

        public WeakRefDelegateInvokeResult Invoke(object arg)
        {
            object output;
            var result = Invoke(arg, out output);
            return result;
        }

        public WeakRefDelegateInvokeResult Invoke<T>(T arg)
        {
            object output;
            var result = Invoke(arg, out output);
            return result;
        }

        public WeakRefDelegateInvokeResult Invoke(object[] args, out object result)
        {
            result = null;
            if (_isDisposed)
            {
                return WeakRefDelegateInvokeResult.Disposed;
            }

            if(!IsAlive)
            {
                return WeakRefDelegateInvokeResult.NotAlive;
            }
            result = _method.Invoke(Target, args);
            return WeakRefDelegateInvokeResult.Success;
        }

        public WeakRefDelegateInvokeResult Invoke<T>(T arg, out object result)
        {
            result = null;
            if (_isDisposed)
            {
                return WeakRefDelegateInvokeResult.Disposed;
            }

            var status = Invoke(new [] { arg }, out result);
            return status;
        }

        public WeakRefDelegateInvokeResult Invoke<T>(T[] args, out object result)
        {
            result = null;
            if (_isDisposed)
            {
                return WeakRefDelegateInvokeResult.Disposed;
            }

            var status = Invoke(args.ToObjectArray(), out result);
            return status;
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
    }
}
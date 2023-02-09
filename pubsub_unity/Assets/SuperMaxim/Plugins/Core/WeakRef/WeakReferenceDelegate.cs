using System;
using System.Reflection;
using SuperMaxim.Core.Extensions;
using UnityEngine;

namespace SuperMaxim.Core.WeakRef
{
    /// <summary>
    /// Weak Reference Delegate Pointer
    /// </summary>
    public sealed class WeakReferenceDelegate
        : IEquatable<WeakReferenceDelegate>, IComparable, IDisposable
    {
        private readonly int _id;

        private MethodInfo _method;

        private WeakReference _ref;

        public object Target
        {
            get
            {
                var target = IsAlive ? _ref?.Target : null;
                return target;
            }
        }

        public bool IsAlive
        {
            get
            {
                if(_method == null)
                {
                    return false;
                }
                if(_method.IsStatic)
                {
                    return true;
                }
                var isAlive = _ref is {IsAlive: true, Target: { }};
                return isAlive;
            }
        }

        public WeakReferenceDelegate(Delegate method)
        {
            _id = method.GetHashCode();
            _method = method.Method;

            if (method.Target != null)
            {
                _ref = new WeakReference(method.Target);
            }
        }

        public object Invoke(object[] args)
        {
            if (_isDisposed || !IsAlive)
            {
                return null;
            }

            _method.ThrowIfNull(nameof(_method));

            var parameters = _method.GetParameters();
            switch (parameters.Length)
            {
                case > 0 when args.IsNullOrEmpty():
                    throw new OperationCanceledException($"The target method has {parameters.Length} params, " +
                                                         $"but passed arguments are null/empty.");
                case > 0 when parameters.Length != args.Length:
                    throw new OperationCanceledException($"The target method has {parameters.Length} param(s), " +
                                                         $"but passed arguments have {args.Length} param(s).");
            }
            for (var i = 0; i < parameters.Length; i++)
            {
                var param = parameters[i];
                var arg = args[i];
                var argType = arg.GetType();
                if (param.ParameterType != argType && param.ParameterType != typeof(object))
                {
                    throw new OperationCanceledException(
                        $"The target method parameter in place #{param.Position} is type of '{param.ParameterType}', " +
                        $"but passed argument is type of '{argType}'.");
                }
            }

            object result;
            if (_method.ReturnType == typeof (void))
            {
                result = null;
                _method.Invoke(Target, args);
            }
            else
            {
                result = _method.Invoke(Target, args);
            }
            return result;
        }

        #region Override Base Methods

        public override int GetHashCode()
        {
            return _id;
        }

        public override string ToString()
        {
            return $"{GetType().Name}: {_id}, {_method}, {IsAlive}";
        }

        #endregion

        #region IDisposable Support
        private bool _isDisposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }
            if (_ref != null && disposing)
            {
                if (_ref.IsAlive && _ref.Target != null)
                {
                    _ref.Target = null;
                }
                _ref = null;
            }

            _method = null;
            _isDisposed = true;
        }

        ~WeakReferenceDelegate()
        {
            Dispose();
        }
        #endregion

        #region IComparable Support

        public int CompareTo(object obj)
        {
            if (obj is not WeakReferenceDelegate @delegate)
            {
                return -1;
            }

            if (Equals(@delegate))
            {
                return 0;
            }
            return -1;
        }

        public static bool operator == (WeakReferenceDelegate left, WeakReferenceDelegate right)
        {
            return Compare(left, right) == 0;
        }

        public static bool operator > (WeakReferenceDelegate left, WeakReferenceDelegate right)
        {
            return Compare(left, right) > 0;
        }

        public static bool operator < (WeakReferenceDelegate left, WeakReferenceDelegate right)
        {
            return Compare(left, right) < 0;
        }

        public static bool operator != (WeakReferenceDelegate left, WeakReferenceDelegate right)
        {
            return !(left == right);
        }

        public static bool operator <= (WeakReferenceDelegate left, WeakReferenceDelegate right)
        {
            return Compare(left, right) <= 0;
        }

        public static bool operator >= (WeakReferenceDelegate left, WeakReferenceDelegate right)
        {
            return Compare(left, right) >= 0;
        }

        private static int Compare(WeakReferenceDelegate left, WeakReferenceDelegate right)
        {
            if (ReferenceEquals(left, right)) return 0;
            if (ReferenceEquals(left, null)) return -1;
            if (ReferenceEquals(right, null)) return 1;

            if (left._id > right._id) return 1;
            if (left._id < right._id) return -1;
            return 0;
        }

        #endregion

        #region IEquatable Support

        public bool Equals(WeakReferenceDelegate other)
        {
            if (other == null)
            {
                return false;
            }
            var otherId = other.GetHashCode();
            var equals = _id == otherId;
            return equals;
        }

        public override bool Equals(object obj)
        {
            if (obj is not WeakReferenceDelegate other)
            {
                return false;
            }
            var isSame = _id == other._id && ReferenceEquals(Target, other.Target);
            return isSame;
        }

        #endregion
    }
}

using System;

namespace SuperMaxim.Core.WeakRef
{
    public class WeakRefWrapper : IDisposable
    {
        private WeakReference _ref;

        public object Target
        {
            get
            {
                object target = IsAlive ? _ref.Target : null;
                return target;
            }
            set
            {
                if(_ref != null)
                {
                    _ref.Target = value;
                }
            }
        }

        public bool IsAlive
        {
            get
            {
                var isAlive = _ref == null || (_ref.IsAlive && _ref.Target != null);
                return isAlive;
            }
        }

        public WeakRefWrapper(object target)
        {
            _ref = new WeakReference(target, false);
        }

        #region IDisposable Support
        protected bool _isDisposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    if(_ref != null)
                    {
                        _ref.Target = null;
                        _ref = null;
                    }
                }
                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}

using System;

namespace SuperMaxim.Core.WeakRef
{
    public abstract class WeakRefWrapper : IDisposable
    {
        private WeakReference _ref;

        protected object Target
        {
            get
            {
                object target = IsAlive ? _ref.Target : null;
                return target;
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

        protected WeakRefWrapper(object target)
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

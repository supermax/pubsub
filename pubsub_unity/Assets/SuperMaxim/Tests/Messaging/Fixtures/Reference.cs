using System;

namespace SuperMaxim.Tests.Messaging.Fixtures
{
    public sealed class Reference<T> : IDisposable where T : class
    {
        public T Ref { get; set; }

        public bool IsDisposed { get; private set; }

        ~Reference()
        {
            Dispose();
        }

        #region IDisposable Support

        /// <summary>
        /// Dispose this instance
        /// </summary>
        private void Dispose(bool disposing)
        {
            if (IsDisposed || !disposing) return;

            Ref = default;
            IsDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}

// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Notifo.Infrastructure
{
    public abstract class DisposableObjectBase : IDisposable
    {
        private readonly object disposeLock = new object();

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (IsDisposed)
            {
                return;
            }

            lock (disposeLock)
            {
                if (!IsDisposed)
                {
                    DisposeObject(disposing);
                }
            }

            IsDisposed = true;
        }

        protected abstract void DisposeObject(bool disposing);

        protected void ThrowIfDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }
    }
}

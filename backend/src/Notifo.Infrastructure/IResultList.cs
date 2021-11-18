// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Infrastructure
{
    public interface IResultList<out T> : IReadOnlyList<T>
    {
        long Total { get; }
    }
}

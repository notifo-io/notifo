// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Infrastructure
{
    public interface ICommand<T> where T : notnull
    {
        bool CanCreate => false;

        ValueTask<T?> ExecuteAsync(T target, IServiceProvider serviceProvider,
            CancellationToken ct);
    }
}

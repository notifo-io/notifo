// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure.Mediator;

namespace Notifo.Domain;

public abstract class AppCommandBase<T> : AppCommandBase, IRequest<T?> where T : notnull
{
    public virtual ValueTask<T?> ExecuteAsync(T target, IServiceProvider serviceProvider,
        CancellationToken ct)
    {
        return default;
    }

    public virtual ValueTask ExecuteAsync(IServiceProvider serviceProvider,
        CancellationToken ct)
    {
        return default;
    }
}

public abstract class AppCommandBase : CommandBase
{
    public string AppId { get; set; }
}

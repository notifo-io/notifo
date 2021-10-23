// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Notifo.Infrastructure
{
    public interface ICommand<in T>
    {
        bool CanCreate => false;

        Task<bool> ExecuteAsync(T target, IServiceProvider serviceProvider,
            CancellationToken ct);

        Task ExecutedAsync(T target, IServiceProvider serviceProvider,
            CancellationToken ct)
        {
            return Task.CompletedTask;
        }
    }
}

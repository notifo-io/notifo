// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using Notifo.Infrastructure;

namespace Notifo.Domain.ChannelTemplates
{
    public sealed class CreateChannelTemplate<T> : ICommand<ChannelTemplate<T>>
    {
        public bool CanCreate => true;

        public Task<bool> ExecuteAsync(ChannelTemplate<T> template, IServiceProvider serviceProvider,
            CancellationToken ct)
        {
            return Task.FromResult(true);
        }
    }
}

// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Notifo.Infrastructure;

namespace Notifo.Domain.ChannelTemplates
{
    public sealed class CreateChannelTemplate<T> : ICommand<ChannelTemplate<T>>
    {
        public bool CanCreate => true;

        public string? Language { get; set; }

        public async Task<bool> ExecuteAsync(ChannelTemplate<T> template, IServiceProvider serviceProvider,
            CancellationToken ct)
        {
            if (Language != null)
            {
                var factory = serviceProvider.GetRequiredService<IChannelTemplateFactory<T>>();

                template.Languages[Language] = await factory.CreateInitialAsync();
            }

            return true;
        }
    }
}

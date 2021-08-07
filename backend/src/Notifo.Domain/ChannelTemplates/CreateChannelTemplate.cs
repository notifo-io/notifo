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
using NodaTime;
using Notifo.Infrastructure;

namespace Notifo.Domain.ChannelTemplates
{
    public sealed class CreateChannelTemplate<T> : ICommand<ChannelTemplate<T>>
    {
        public string? Language { get; set; }

        public async Task<bool> ExecuteAsync(ChannelTemplate<T> template, IServiceProvider serviceProvider,
            CancellationToken ct)
        {
            var factory = serviceProvider.GetRequiredService<IChannelTemplateFactory<T>>();

            var clock = serviceProvider.GetRequiredService<IClock>();

            if (Language != null)
            {
                if (template.Languages.ContainsKey(Language))
                {
                    throw new DomainObjectConflictException(Language);
                }

                template.Languages[Language] = await factory.CreateInitialAsync();
                template.LastUpdate = clock.GetCurrentInstant();
            }
            else
            {
                template.LastUpdate = clock.GetCurrentInstant();
            }

            return true;
        }
    }
}

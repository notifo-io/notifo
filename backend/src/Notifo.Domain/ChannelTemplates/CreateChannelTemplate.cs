// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Collections;

namespace Notifo.Domain.ChannelTemplates
{
    public sealed class CreateChannelTemplate<T> : ICommand<ChannelTemplate<T>>
    {
        public bool CanCreate => true;

        public string? Language { get; set; }

        public async ValueTask<ChannelTemplate<T>?> ExecuteAsync(ChannelTemplate<T> template, IServiceProvider serviceProvider,
            CancellationToken ct)
        {
            if (Language != null)
            {
                var factory = serviceProvider.GetRequiredService<IChannelTemplateFactory<T>>();

                template = template with
                {
                    Languages = new Dictionary<string, T>(template.Languages)
                    {
                        [Language] = await factory.CreateInitialAsync()
                    }.ToReadonlyDictionary()
                };
            }

            return template;
        }
    }
}

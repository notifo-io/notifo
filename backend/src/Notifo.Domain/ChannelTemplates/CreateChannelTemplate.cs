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
        public string? Language { get; set; }

        public string? Kind { get; set; }

        public bool CanCreate => true;

        public async ValueTask<ChannelTemplate<T>?> ExecuteAsync(ChannelTemplate<T> template, IServiceProvider serviceProvider,
            CancellationToken ct)
        {
            var newTemplate = template;

            if (Kind != null && !string.Equals(Kind, template.Name, StringComparison.Ordinal))
            {
                newTemplate = newTemplate with
                {
                    Kind = Kind.Trim()
                };
            }

            if (Language != null)
            {
                var factory = serviceProvider.GetRequiredService<IChannelTemplateFactory<T>>();

                newTemplate = newTemplate with
                {
                    Languages = new Dictionary<string, T>(template.Languages)
                    {
                        [Language] = await factory.CreateInitialAsync(newTemplate.Kind, ct)
                    }.ToReadonlyDictionary()
                };
            }

            return newTemplate;
        }
    }
}

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
    public sealed class UpdateChannelTemplate<T> : ICommand<ChannelTemplate<T>>
    {
        public string? Name { get; set; }

        public bool? Primary { get; set; }

        public Dictionary<string, T>? Languages { get; set; }

        public async ValueTask<ChannelTemplate<T>?> ExecuteAsync(ChannelTemplate<T> template, IServiceProvider serviceProvider,
            CancellationToken ct)
        {
            var newTemplate = template;

            if (Languages != null)
            {
                var languages = new Dictionary<string, T>();

                var factory = serviceProvider.GetRequiredService<IChannelTemplateFactory<T>>();

                foreach (var (key, value) in Languages)
                {
                    languages[key] = await factory.ParseAsync(value);
                }

                newTemplate = newTemplate with
                {
                    Languages = languages.ToReadonlyDictionary()
                };
            }

            if (Name != null && !string.Equals(Name, template.Name, StringComparison.Ordinal))
            {
                newTemplate = newTemplate with
                {
                    Name = Name.Trim()
                };
            }

            if (Primary.HasValue && Primary != template.Primary)
            {
                newTemplate = newTemplate with
                {
                    Primary = Primary.Value
                };
            }

            return newTemplate;
        }
    }
}

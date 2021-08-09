// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Notifo.Infrastructure;

namespace Notifo.Domain.ChannelTemplates
{
    public sealed class UpdateChannelTemplate<T> : ICommand<ChannelTemplate<T>>
    {
        public string? Name { get; set; }

        public bool? Primary { get; set; }

        public Dictionary<string, T>? Languages { get; set; }

        public async Task<bool> ExecuteAsync(ChannelTemplate<T> template, IServiceProvider serviceProvider,
            CancellationToken ct)
        {
            if (Name != null)
            {
                template.Name = Name;
            }

            if (Primary != null)
            {
                template.Primary = Primary.Value;
            }

            if (Languages != null)
            {
                var factory = serviceProvider.GetRequiredService<IChannelTemplateFactory<T>>();

                foreach (var (key, value) in Languages.ToList())
                {
                    Languages[key] = await factory.ParseAsync(value);
                }

                template.Languages = Languages;
            }

            return true;
        }
    }
}

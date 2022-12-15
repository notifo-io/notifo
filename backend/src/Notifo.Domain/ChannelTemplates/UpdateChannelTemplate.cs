// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Notifo.Domain.Utils;
using Notifo.Infrastructure.Collections;

namespace Notifo.Domain.ChannelTemplates;

public sealed class UpdateChannelTemplate<T> : ChannelTemplateCommand<T>
{
    public string? Name { get; set; }

    public bool? Primary { get; set; }

    public Dictionary<string, T>? Languages { get; set; }

    public override async ValueTask<ChannelTemplate<T>?> ExecuteAsync(ChannelTemplate<T> target, IServiceProvider serviceProvider,
        CancellationToken ct)
    {
        var newTemplate = target;

        if (Languages != null)
        {
            var languages = new Dictionary<string, T>();

            var factory = serviceProvider.GetRequiredService<IChannelTemplateFactory<T>>();

            foreach (var (key, value) in Languages)
            {
                await factory.ParseAsync(value, false, ct);
            }

            newTemplate = newTemplate with
            {
                Languages = languages.ToReadonlyDictionary()
            };
        }

        if (Is.Changed(Name, target.Name))
        {
            newTemplate = newTemplate with
            {
                Name = Name.Trim()
            };
        }

        if (Is.Changed(Primary, target.Primary))
        {
            newTemplate = newTemplate with
            {
                Primary = Primary.Value
            };
        }

        return newTemplate;
    }
}

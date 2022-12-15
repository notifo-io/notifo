// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Notifo.Infrastructure.Collections;

namespace Notifo.Domain.ChannelTemplates;

public sealed class CreateChannelTemplate<T> : ChannelTemplateCommand<T>
{
    public string? Language { get; set; }

    public override bool CanCreate => true;

    public override async ValueTask<ChannelTemplate<T>?> ExecuteAsync(ChannelTemplate<T> target, IServiceProvider serviceProvider,
        CancellationToken ct)
    {
        var newTemplate = target;

        if (Language != null)
        {
            var channelFactory = serviceProvider.GetRequiredService<IChannelTemplateFactory<T>>();
            var channelInstance = await channelFactory.CreateInitialAsync(ct);

            newTemplate = newTemplate with
            {
                Languages = target.Languages.Set(Language, channelInstance)
            };
        }

        return newTemplate;
    }
}

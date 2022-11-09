// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure;
using Notifo.Infrastructure.Texts;

namespace Notifo.Domain.Templates;

public sealed class UpsertTemplate : ICommand<Template>
{
    public NotificationFormatting<LocalizedText>? Formatting { get; set; }

    public ChannelSettings? Settings { get; set; }

    public bool CanCreate => true;

    public ValueTask<Template?> ExecuteAsync(Template target, IServiceProvider serviceProvider,
        CancellationToken ct)
    {
        var newTemplate = target with
        {
            IsAutoCreated = false
        };

        if (Formatting != null)
        {
            newTemplate = newTemplate with
            {
                Formatting = Formatting
            };
        }

        if (Settings != null)
        {
            newTemplate = newTemplate with
            {
                Settings = Settings
            };
        }

        return new ValueTask<Template?>(newTemplate);
    }
}

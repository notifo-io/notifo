// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure;
using Notifo.Infrastructure.Texts;

namespace Notifo.Domain.Templates
{
    public sealed class UpdateTemplate : ICommand<Template>
    {
        public NotificationFormatting<LocalizedText>? Formatting { get; set; }

        public NotificationSettings Settings { get; } = new NotificationSettings();

        public ValueTask<Template?> ExecuteAsync(Template target, IServiceProvider serviceProvider, CancellationToken ct)
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
            else if (newTemplate.Formatting == null)
            {
                newTemplate = newTemplate with
                {
                    Formatting = new NotificationFormatting<LocalizedText>()
                };
            }

            if (Settings != null)
            {
                newTemplate = newTemplate with
                {
                    Settings = Settings
                };
            }
            else if (newTemplate.Settings == null)
            {
                newTemplate = newTemplate with
                {
                    Settings = new NotificationSettings()
                };
            }

            return new ValueTask<Template?>(newTemplate);
        }
    }
}

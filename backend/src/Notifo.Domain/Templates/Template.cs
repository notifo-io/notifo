// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Infrastructure.Texts;

namespace Notifo.Domain.Templates
{
    public sealed record Template
    {
        public string AppId { get; init; }

        public string Code { get; init; }

        public bool IsAutoCreated { get; init; }

        public Instant Created { get; init; }

        public Instant LastUpdate { get; init; }

        public NotificationFormatting<LocalizedText> Formatting { get; init; }

        public NotificationSettings Settings { get; init; }

        public static Template Create(string appId, string code, Instant now)
        {
            return new Template { AppId = appId, Code = code, Created = now };
        }

        public Template Update(UpdateTemplate update)
        {
            var newTemplate = this with
            {
                IsAutoCreated = false
            };

            if (update.Formatting != null)
            {
                newTemplate = newTemplate with
                {
                    Formatting = update.Formatting
                };
            }
            else if (newTemplate.Formatting == null)
            {
                newTemplate = newTemplate with
                {
                    Formatting = new NotificationFormatting<LocalizedText>()
                };
            }

            if (update.Settings != null)
            {
                newTemplate = newTemplate with
                {
                    Settings = update.Settings
                };
            }
            else if (newTemplate.Settings == null)
            {
                newTemplate = newTemplate with
                {
                    Settings = new NotificationSettings()
                };
            }

            return newTemplate;
        }
    }
}

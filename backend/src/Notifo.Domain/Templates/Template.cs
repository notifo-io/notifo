// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure.Texts;

namespace Notifo.Domain.Templates
{
    public sealed class Template
    {
        public string AppId { get; set; }

        public string Code { get; set; }

        public bool IsAutoCreated { get; set; }

        public NotificationFormatting<LocalizedText> Formatting { get; set; }

        public NotificationSettings Settings { get; set; }

        public static Template Create(string appId, string code)
        {
            var user = new Template { AppId = appId, Code = code };

            return user;
        }

        public void Update(TemplateUpdate update)
        {
            IsAutoCreated = false;

            if (update.Formatting != null)
            {
                Formatting = update.Formatting;
            }
            else
            {
                Formatting ??= new NotificationFormatting<LocalizedText>();
            }

            if (update.Settings != null)
            {
                Settings = update.Settings;
            }
            else
            {
                Settings ??= new NotificationSettings();
            }
        }
    }
}

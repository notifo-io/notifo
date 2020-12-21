// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure.Texts;

namespace Notifo.Domain.Templates
{
    public sealed class TemplateUpdate
    {
        public NotificationFormatting<LocalizedText>? Formatting { get; set; }

        public NotificationSettings Settings { get; } = new NotificationSettings();
    }
}

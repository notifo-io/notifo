// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Infrastructure.Texts;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Notifo.Domain.Templates
{
    public sealed record Template(string AppId, string Code, Instant Created)
    {
        public bool IsAutoCreated { get; init; }

        public Instant LastUpdate { get; init; }

        public NotificationFormatting<LocalizedText> Formatting { get; init; } = new NotificationFormatting<LocalizedText>();

        public NotificationSettings Settings { get; init; } = new NotificationSettings();
    }
}

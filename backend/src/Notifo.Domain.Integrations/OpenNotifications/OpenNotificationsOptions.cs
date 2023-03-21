// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Integrations.OpenNotifications;

public sealed class OpenNotificationsOptions
{
    public Service[]? Services { get; set; }

    public sealed class Service
    {
        public string Name { get; set; }

        public string Url { get; set; }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Name) && Uri.IsWellFormedUriString(Url, UriKind.Absolute);
        }
    }
}

// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;

namespace Notifo.Domain.Channels.Email
{
    public sealed class NamedEmailTemplate
    {
        public string? Name { get; set; }

        public bool Primary { get; set; }

        public Instant LastUpdate { get; set; }

        public Dictionary<string, EmailTemplate> Languages { get; set; } = new Dictionary<string, EmailTemplate>();
    }
}

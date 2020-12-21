// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Notifo.Domain.Integrations.MessageBird
{
    public sealed class MessageBirdOptions
    {
        public string AccessKey { get; set; }

        public string PhoneNumber { get; set; }

        public Dictionary<string, string>? PhoneNumbers { get; set; }
    }
}

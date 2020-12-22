// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Notifo.Infrastructure.Configuration;

namespace Notifo.Domain.Integrations.MessageBird
{
    public sealed class MessageBirdOptions : IValidatableOptions
    {
        public string AccessKey { get; set; }

        public string PhoneNumber { get; set; }

        public Dictionary<string, string>? PhoneNumbers { get; set; }

        public IEnumerable<ConfigurationError> Validate()
        {
            if (string.IsNullOrWhiteSpace(AccessKey))
            {
                yield return new ConfigurationError("Value is required.", nameof(AccessKey));
            }

            if (string.IsNullOrWhiteSpace(PhoneNumber))
            {
                yield return new ConfigurationError("Value is required.", nameof(PhoneNumber));
            }
        }
    }
}

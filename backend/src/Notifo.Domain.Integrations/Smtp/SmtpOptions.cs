// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Hosting.Configuration;

namespace Notifo.Domain.Integrations.Smtp
{
    public class SmtpOptions : IValidatableOptions
    {
        public string Host { get; set; }

        public string? Username { get; set; }

        public string? Password { get; set; }

        public int HostPort { get; set; } = 587;

        public bool IsValid()
        {
            return !Validate().Any();
        }

        public virtual IEnumerable<ConfigurationError> Validate()
        {
            if (string.IsNullOrWhiteSpace(Host))
            {
                yield return new ConfigurationError("Value is required.", nameof(Host));
            }

            if (HostPort == 0)
            {
                yield return new ConfigurationError("Value is required.", nameof(HostPort));
            }
        }
    }
}

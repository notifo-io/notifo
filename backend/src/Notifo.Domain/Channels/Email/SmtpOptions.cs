// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Hosting.Configuration;

namespace Notifo.Domain.Channels.Email
{
    public class SmtpOptions : IValidatableOptions
    {
        public string Host { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public int Port { get; set; } = 587;

        public virtual IEnumerable<ConfigurationError> Validate()
        {
            if (string.IsNullOrWhiteSpace(Host))
            {
                yield return new ConfigurationError("Value is required.", nameof(Host));
            }

            if (string.IsNullOrWhiteSpace(Username))
            {
                yield return new ConfigurationError("Value is required.", nameof(Username));
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                yield return new ConfigurationError("Value is required.", nameof(Password));
            }

            if (Port != 0)
            {
                yield return new ConfigurationError("Value is required.", nameof(Port));
            }
        }
    }
}

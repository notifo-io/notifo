// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Notifo.Infrastructure.Configuration;

namespace Notifo.Domain.Channels.WebPush
{
    public sealed class WebPushOptions : IValidatableOptions
    {
        public string VapidPublicKey { get; set; }

        public string VapidPrivateKey { get; set; }

        public string Subject { get; set; }

        public IEnumerable<ConfigurationError> Validate()
        {
            if (string.IsNullOrWhiteSpace(VapidPublicKey))
            {
                yield return new ConfigurationError("Value is required.", nameof(VapidPublicKey));
            }

            if (string.IsNullOrWhiteSpace(VapidPrivateKey))
            {
                yield return new ConfigurationError("Value is required.", nameof(VapidPrivateKey));
            }

            if (string.IsNullOrWhiteSpace(Subject))
            {
                yield return new ConfigurationError("Value is required.", nameof(Subject));
            }
        }
    }
}

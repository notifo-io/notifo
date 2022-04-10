// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Hosting.Configuration;

namespace Notifo.Domain.Channels.WebPush
{
    public sealed class WebPushOptions : IValidatableOptions
    {
        public string VapidPublicKey { get; init; }

        public string VapidPrivateKey { get; init; }

        public string Subject { get; init; }

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

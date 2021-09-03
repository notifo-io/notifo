// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Hosting.Configuration;

namespace Notifo.Infrastructure.Messaging.Implementation.GooglePubSub
{
    public sealed class GooglePubSubOptions : IValidatableOptions
    {
        public string ProjectId { get; set; }

        public string Prefix { get; set; }

        public IEnumerable<ConfigurationError> Validate()
        {
            if (string.IsNullOrWhiteSpace(ProjectId))
            {
                yield return new ConfigurationError("Value is required.", nameof(ProjectId));
            }

            if (string.IsNullOrWhiteSpace(Prefix))
            {
                yield return new ConfigurationError("Value is required.", nameof(Prefix));
            }
        }
    }
}

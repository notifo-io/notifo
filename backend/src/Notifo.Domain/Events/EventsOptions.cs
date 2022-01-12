// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Hosting.Configuration;

namespace Notifo.Domain.Events
{
    public sealed class EventsOptions : IValidatableOptions
    {
        public TimeSpan RetentionTime { get; set; } = TimeSpan.FromDays(10);

        public IEnumerable<ConfigurationError> Validate()
        {
            yield break;
        }
    }
}

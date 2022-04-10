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
        public TimeSpan RetentionTime { get; init; } = TimeSpan.FromDays(10);

        public IEnumerable<ConfigurationError> Validate()
        {
            if (RetentionTime <= TimeSpan.Zero)
            {
                yield return new ConfigurationError("Retention time must be greater than zero.", nameof(RetentionTime));
            }
        }
    }
}

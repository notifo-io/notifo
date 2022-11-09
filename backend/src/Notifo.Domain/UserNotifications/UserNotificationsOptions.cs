// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Hosting.Configuration;

namespace Notifo.Domain.UserNotifications;

public sealed class UserNotificationsOptions : IValidatableOptions
{
    public TimeSpan RetentionTime { get; init; } = TimeSpan.FromDays(20);

    public int MaxItemsPerUser { get; init; } = 5000;

    public IEnumerable<ConfigurationError> Validate()
    {
        if (RetentionTime <= TimeSpan.Zero)
        {
            yield return new ConfigurationError("Retention time must be greater than zero.", nameof(RetentionTime));
        }
    }
}

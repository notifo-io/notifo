// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Hosting.Configuration;

namespace Notifo.Domain.UserNotifications
{
    public sealed class UserNotificationsOptions : IValidatableOptions
    {
        public TimeSpan RetentionTime { get; set; } = TimeSpan.FromDays(20);

        public IEnumerable<ConfigurationError> Validate()
        {
            yield break;
        }
    }
}

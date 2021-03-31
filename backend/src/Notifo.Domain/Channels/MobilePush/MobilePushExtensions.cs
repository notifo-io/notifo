﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;

namespace Notifo.Domain.Channels.MobilePush
{
    public static class MobilePushExtensions
    {
        public static readonly Duration WakeupDelay = Duration.FromSeconds(30);
        private static readonly Duration TimeBetweenWakeup = Duration.FromMinutes(30);

        public static Instant? GetNextWakeupTime(this MobilePushToken token, IClock clock)
        {
            var now = clock.GetCurrentInstant().Plus(WakeupDelay);

            if (token.LastWakeup > now)
            {
                return null;
            }

            var nextWakeup = now;

            var timeSinceLastWakeUp = now - token.LastWakeup;

            if (timeSinceLastWakeUp < TimeBetweenWakeup)
            {
                nextWakeup = token.LastWakeup + TimeBetweenWakeup;
            }

            return nextWakeup;
        }
    }
}

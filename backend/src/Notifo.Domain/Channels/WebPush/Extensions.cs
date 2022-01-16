// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;

namespace Notifo.Domain.Channels.WebPush
{
    public static class Extensions
    {
        public static IEnumerable<ActivityLink> Links(this WebPushJob job)
        {
            if (job.UserEventActivity != default)
            {
                yield return new ActivityLink(job.UserEventActivity);
            }

            if (job.EventActivity != default)
            {
                yield return new ActivityLink(job.EventActivity);
            }

            if (job.NotificationActivity != default)
            {
                yield return new ActivityLink(job.NotificationActivity);
            }
        }
    }
}

// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;
using Notifo.Domain.Channels.Sms;

namespace Notifo.Domain.Channels.Sms
{
    public static class Extensions
    {
        public static IEnumerable<ActivityLink> Links(this SmsJob job)
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

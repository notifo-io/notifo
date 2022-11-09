// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;
using Notifo.Domain.Events;
using Notifo.Domain.UserEvents;
using Notifo.Domain.UserNotifications;

namespace Notifo.Domain
{
    public static class TelemetryExtensions
    {
        public static IEnumerable<ActivityLink> Links(this UserEventMessage message)
        {
            if (message.UserEventActivity != default)
            {
                yield return new ActivityLink(message.UserEventActivity);
            }

            if (message.EventActivity != default)
            {
                yield return new ActivityLink(message.EventActivity);
            }
        }

        public static IEnumerable<ActivityLink> Links(this EventMessage message)
        {
            if (message.EventActivity != default)
            {
                yield return new ActivityLink(message.EventActivity);
            }
        }

        public static IEnumerable<ActivityLink> Links(this BaseUserNotification notification)
        {
            if (notification.UserEventActivity != default)
            {
                yield return new ActivityLink(notification.UserEventActivity);
            }

            if (notification.EventActivity != default)
            {
                yield return new ActivityLink(notification.EventActivity);
            }

            if (notification.UserNotificationActivity != default)
            {
                yield return new ActivityLink(notification.UserNotificationActivity);
            }
        }
    }
}

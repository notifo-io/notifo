// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Notifo.Domain.UserNotifications;

namespace Notifo.Domain.Channels.MobilePush
{
    public sealed class MobilePushJob
    {
        public UserNotification Notification { get; set; }

        public HashSet<string> Tokens { get; set; }

        public string ScheduleKey
        {
            get { return Notification.Id.ToString(); }
        }

        public MobilePushJob()
        {
        }

        public MobilePushJob(UserNotification notification, HashSet<string> tokens)
        {
            Notification = notification;

            Tokens = tokens;
        }

        public static string ComputeScheduleKey(Guid id)
        {
            return id.ToString();
        }
    }
}

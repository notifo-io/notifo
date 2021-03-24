// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using Notifo.Domain.UserNotifications;
using Notifo.Domain.Users;

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

        public MobilePushJob(UserNotification notification, User user)
        {
            Notification = notification;

            Tokens = user.MobilePushTokens.Select(x => x.Token).ToHashSet();
        }
    }
}

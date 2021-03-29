// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Notifo.Domain.UserNotifications;
using Notifo.Domain.Users;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Domain.Channels.Sms
{
    public sealed class SmsJob : IUserNotification
    {
        public Guid Id { get; set; }

        public string PhoneNumber { get; set; }

        public string AppId { get; set; }

        public string EventId { get; set; }

        public string UserId { get; set; }

        public string Topic { get; set; }

        public string Text { get; set; }

        public string ScheduleKey
        {
            get { return Id.ToString(); }
        }

        public SmsJob()
        {
        }

        public SmsJob(UserNotification notification, User user)
        {
            PhoneNumber = user.PhoneNumber;

            SimpleMapper.Map(notification, this);

            Text = notification.Formatting.Subject;

            if (Text.Length > 140)
            {
                Text = Text.Substring(0, 137) + "...";
            }
        }

        public static string ComputeScheduleKey(Guid id)
        {
            return id.ToString();
        }
    }
}

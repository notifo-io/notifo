// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure;

namespace Notifo.Domain.Channels.Sms;

public sealed class SmsJob : ChannelJob
{
    public string ScheduleKey
    {
        get => string.Join("_",
            Notification.AppId,
            Notification.UserId,
            Template,
            GroupKey.OrDefault(Notification.Id));
    }

    public SmsJob()
    {
    }

    public SmsJob(UserNotification notification, ChannelContext context)
        : base(notification, context)
    {
    }
}

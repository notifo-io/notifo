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
    public string PhoneNumber { get; init; }

    public string? Template { get; init; }

    public string ScheduleKey
    {
        get => string.Join("_",
            Notification.AppId,
            Notification.UserId,
            GroupKey.OrDefault(Notification.Id),
            PhoneNumber);
    }

    public SmsJob()
    {
    }

    public SmsJob(UserNotification notification, ChannelContext context, string phoneNumber)
        : base(notification, context)
    {
        Template = context.Setting.Template;

        PhoneNumber = phoneNumber;
    }
}

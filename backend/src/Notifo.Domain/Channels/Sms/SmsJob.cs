// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.UserNotifications;

namespace Notifo.Domain.Channels.Sms;

public sealed class SmsJob : ChannelJob
{
    public string PhoneNumber { get; init; }

    public string? Template { get; init; }

    public string ScheduleKey
    {
        get => ComputeScheduleKey(Notification.Id);
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

    public static string ComputeScheduleKey(Guid notificationId)
    {
        return notificationId.ToString();
    }
}

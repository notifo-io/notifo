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

    public string TemplateLanguage { get; init; }

    public string? TemplateName { get; init; }

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
        PhoneNumber = phoneNumber;
        TemplateLanguage = notification.UserLanguage;
        TemplateName = context.Setting.Template;
    }

    public static string ComputeScheduleKey(Guid notificationId)
    {
        return notificationId.ToString();
    }
}

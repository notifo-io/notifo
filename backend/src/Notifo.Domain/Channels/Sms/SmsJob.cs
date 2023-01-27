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
    public string SmsNumber { get; init; }

    public string SmsText { get; init; }

    public string TemplateLanguage { get; init; }

    public string? TemplateName { get; init; }

    public string ScheduleKey
    {
        get => ComputeScheduleKey(Notification.Id, SmsNumber);
    }

    public SmsJob()
    {
    }

    public SmsJob(UserNotification notification, ChannelContext context, string phoneNumber)
        : base(notification, context)
    {
        SmsNumber = phoneNumber;
        SmsText = notification.Formatting.Subject;
        TemplateLanguage = notification.UserLanguage;
        TemplateName = context.Setting.Template;
    }

    public static string ComputeScheduleKey(Guid notificationId, string phoneNumber)
    {
        return $"{notificationId}_{phoneNumber}";
    }
}

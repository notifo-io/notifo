// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Integrations;
using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure;

namespace Notifo.Domain.Channels.Email;

public sealed class EmailJob : ChannelJob
{
    public BaseUserNotification Notification { get; init; }

    public string EmailAddress { get; init; }

    public string? EmailTemplate { get; init; }

    public string? FromEmail { get; init; }

    public string? FromName { get; init; }

    public string ScheduleKey
    {
        get => string.Join("_",
            Notification.AppId,
            Notification.UserId,
            Notification.UserLanguage,
            Notification.Test,
            EmailAddress,
            EmailTemplate,
            FromEmail,
            FromName);
    }

    public EmailJob()
    {
    }

    public EmailJob(BaseUserNotification notification, ChannelSetting setting, Guid configurationId, string emailAddress)
        : base(notification, setting, configurationId, false, Providers.Email)
    {
        EmailAddress = emailAddress;
        EmailTemplate = setting.Template;
        FromEmail = setting.Properties?.GetOrDefault(nameof(FromEmail));
        FromName = setting.Properties?.GetOrDefault(nameof(FromName));
        Notification = notification;
    }
}

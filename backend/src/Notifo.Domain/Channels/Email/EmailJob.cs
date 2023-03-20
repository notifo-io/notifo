// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure;

namespace Notifo.Domain.Channels.Email;

public sealed class EmailJob : ChannelJob
{
    public string EmailAddress { get; init; }

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
            Template,
            FromEmail,
            FromName);
    }

    public EmailJob()
    {
    }

    public EmailJob(UserNotification notification, ChannelContext context, string emailAddress)
        : base(notification, context)
    {
        EmailAddress = emailAddress;
        Template = context.Setting.Template;
        FromEmail = context.Setting.Properties?.GetOrDefault(nameof(FromEmail));
        FromName = context.Setting.Properties?.GetOrDefault(nameof(FromName));
    }
}

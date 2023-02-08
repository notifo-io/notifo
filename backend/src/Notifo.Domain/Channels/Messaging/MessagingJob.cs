// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.UserNotifications;

namespace Notifo.Domain.Channels.Messaging;

public sealed class MessagingJob : ChannelJob
{
    public string? Template { get; init; }

    public SendConfiguration Configuration { get; init; }

    public string ScheduleKey
    {
        get => ComputeScheduleKey(Notification.Id);
    }

    public MessagingJob()
    {
    }

    public MessagingJob(UserNotification notification, ChannelContext context)
        : base(notification, context)
    {
        Template = context.Setting.Template;

        Configuration = context.Configuration;
    }

    public static string ComputeScheduleKey(Guid notificationId)
    {
        return notificationId.ToString();
    }
}

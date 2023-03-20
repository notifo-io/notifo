// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure;

namespace Notifo.Domain.Channels.Messaging;

public sealed class MessagingJob : ChannelJob
{
    public SendConfiguration Configuration { get; init; }

    public string ScheduleKey
    {
        get => string.Join("_",
            Notification.AppId,
            Notification.UserId,
            Template,
            GroupKey.OrDefault(Notification.Id));
    }

    public MessagingJob()
    {
    }

    public MessagingJob(UserNotification notification, ChannelContext context)
        : base(notification, context)
    {
        Configuration = context.Configuration;
    }
}

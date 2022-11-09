// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.UserNotifications;

namespace Notifo.Domain.Channels;

public interface ICommunicationChannel
{
    string Name { get; }

    bool IsSystem => false;

    Task SendAsync(UserNotification notification, ChannelSetting settings, Guid configurationId, SendConfiguration configuration, SendContext context,
        CancellationToken ct);

    IEnumerable<SendConfiguration> GetConfigurations(UserNotification notification, ChannelSetting settings, SendContext context);

    Task HandleDeliveredAsync(TrackingToken token)
    {
        return Task.CompletedTask;
    }

    Task HandleSeenAsync(TrackingToken token)
    {
        return Task.CompletedTask;
    }
}

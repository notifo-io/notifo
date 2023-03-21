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

    bool IsSystem { get; }

    Task HandleSeenAsync(UserNotification notification, ChannelContext context);

    Task SendAsync(UserNotification notification, ChannelContext context,
        CancellationToken ct);

    IEnumerable<SendConfiguration> GetConfigurations(UserNotification notification, ChannelContext context);
}

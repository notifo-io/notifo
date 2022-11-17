// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.UserEvents;

namespace Notifo.Domain.UserNotifications;

public interface IUserNotificationService
{
    Task DistributeAsync(UserEventMessage userEvent);

    Task TrackDeliveredAsync(params TrackingToken[] tokens);

    Task TrackSeenAsync(params TrackingToken[] tokens);

    Task TrackConfirmedAsync(params TrackingToken[] token);
}

// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.SignalR;
using Notifo.Areas.Api.Controllers.Notifications.Dtos;
using Notifo.Domain.Channels.Web;
using Notifo.Domain.Integrations;
using Notifo.Domain.UserNotifications;

namespace Notifo.Areas.Api.Controllers.Notifications;

public sealed class NotificationHubAccessor(IHubContext<NotificationHub> hubContext) : IStreamClient
{
    public Task SendAsync(UserNotification userNotification)
    {
        var dto = UserNotificationDto.FromDomainObject(userNotification, Providers.Web);

        var userId = $"{userNotification.AppId}_{userNotification.UserId}";

        return hubContext.Clients.User(userId).SendAsync("notification", dto);
    }
}

// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.SignalR;
using Notifo.Areas.Api.Controllers.Notifications.Dtos;
using Notifo.Domain.Channels.Web;
using Notifo.Domain.UserNotifications;

namespace Notifo.Areas.Api.Controllers.Notifications;

public sealed class NotificationHubAccessor : IStreamClient
{
    private readonly IHubContext<NotificationHub> hubContext;

    public NotificationHubAccessor(IHubContext<NotificationHub> hubContext)
    {
        this.hubContext = hubContext;
    }

    public Task SendAsync(UserNotification userNotification)
    {
        var dto = UserNotificationDto.FromDomainObject(userNotification);

        var userId = $"{userNotification.AppId}_{userNotification.UserId}";

        return hubContext.Clients.User(userId).SendAsync("notification", dto);
    }
}

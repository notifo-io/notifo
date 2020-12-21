// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Notifo.Areas.Api.Controllers.Notifications.Dto;
using Notifo.Domain.Channels.Web;
using Notifo.Domain.UserNotifications;

namespace Notifo.Areas.Api.Controllers.Notifications
{
    public sealed class NotificationHubAccessor : IStreamClient
    {
        private readonly IHubContext<NotificationHub> hubContext;

        public NotificationHubAccessor(IHubContext<NotificationHub> hubContext)
        {
            this.hubContext = hubContext;
        }

        public Task SendAsync(UserNotification userNotification)
        {
            var dto = NotificationDto.FromNotification(userNotification);

            var userId = $"{userNotification.AppId}_{userNotification.UserId}";

            return hubContext.Clients.User(userId).SendAsync("notification", dto);
        }
    }
}
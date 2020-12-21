// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Notifo.Areas.Api.Controllers.Notifications.Dto;
using Notifo.Domain;
using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure.Security;
using Notifo.Pipeline;

namespace Notifo.Areas.Api.Controllers.Notifications
{
    [AppPermission(Roles.User)]
    public sealed class NotificationHub : Hub
    {
        private readonly IUserNotificationStore userNotificationsStore;
        private readonly IUserNotificationService userNotificationService;

        private string AppId => Context.User!.AppId()!;

        private string UserId => Context.User!.UserId()!;

        public NotificationHub(
            IUserNotificationStore userNotificationsStore,
            IUserNotificationService userNotificationService)
        {
            this.userNotificationsStore = userNotificationsStore;
            this.userNotificationService = userNotificationService;
        }

        public override async Task OnConnectedAsync()
        {
            var notifications = await userNotificationsStore.QueryAsync(AppId, UserId, 100, default, Context.ConnectionAborted);

            var dtos = notifications.Select(NotificationDto.FromNotification).ToArray();

            if (dtos.Length > 0)
            {
                await Clients.Caller.SendAsync("notifications", dtos, Context.ConnectionAborted);
            }
        }

        public async Task ConfirmMany(HandledRequestDto request)
        {
            if (request.Confirmed.HasValue)
            {
                await userNotificationService.TrackConfirmedAsync(request.Confirmed.Value, request.Channel);
            }

            if (request.Seen?.Length > 0)
            {
                await userNotificationService.TrackSeenAsync(request.Seen, request.Channel);
            }
        }
    }
}
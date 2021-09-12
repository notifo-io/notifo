// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Notifo.Areas.Api.Controllers.Notifications.Dtos;
using Notifo.Domain.Identity;
using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure.Security;
using Notifo.Pipeline;

namespace Notifo.Areas.Api.Controllers.Notifications
{
    [AppPermission(NotifoRoles.AppUser)]
    public sealed class NotificationHub : Hub
    {
        private static readonly UserNotificationQuery DefaultQuery = new UserNotificationQuery { Take = 100 };
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
            var notifications = await userNotificationsStore.QueryAsync(AppId, UserId, DefaultQuery, Context.ConnectionAborted);

            var dtos = notifications.Select(UserNotificationDto.FromDomainObject).ToArray();

            await Clients.Caller.SendAsync("notifications", dtos, Context.ConnectionAborted);
        }

        public async Task Delete(Guid id)
        {
            await userNotificationsStore.DeleteAsync(id, Context.ConnectionAborted);

            await Clients.User(Context.User?.Sub()!).SendAsync("notificationDeleted", new { id }, Context.ConnectionAborted);
        }

        public async Task ConfirmMany(TrackNotificationDto request)
        {
            var details = request.ToDetails();

            if (request.Confirmed.HasValue)
            {
                await userNotificationService.TrackConfirmedAsync(request.Confirmed.Value, details);
            }

            if (request.Seen?.Length > 0)
            {
                await userNotificationService.TrackSeenAsync(request.Seen, details);
            }
        }
    }
}

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
        private static readonly UserNotificationQuery DefaultQuery = new UserNotificationQuery { Take = 100 };
        private readonly IUserNotificationStore userNotificationsStore;
        private readonly IUserNotificationService userNotificationService;

        private string AppId
        {
            get
            {
                var id = Context.User?.AppId();

                if (id == null)
                {
                    throw new InvalidOperationException("Not in an authorized context.");
                }

                return id;
            }
        }

        private string UserId
        {
            get
            {
                var id = Context.User?.UserId();

                if (id == null)
                {
                    throw new InvalidOperationException("Not in an authorized context.");
                }

                return id;
            }
        }

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

            var dtos = notifications.Select(NotificationDto.FromNotification).ToArray();

            await Clients.Caller.SendAsync("notifications", dtos, Context.ConnectionAborted);
        }

        public async Task Delete(Guid id)
        {
            await userNotificationsStore.DeleteAsync(id, Context.ConnectionAborted);

            await Clients.User(Context.User?.Sub()!).SendAsync("notificationDeleted", new { id });
        }

        public async Task ConfirmMany(TrackNotificationDto request)
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
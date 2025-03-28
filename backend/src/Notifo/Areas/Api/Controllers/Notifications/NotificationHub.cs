﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.SignalR;
using Notifo.Areas.Api.Controllers.Notifications.Dtos;
using Notifo.Domain;
using Notifo.Domain.Identity;
using Notifo.Domain.Integrations;
using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure.Security;
using Notifo.Pipeline;

namespace Notifo.Areas.Api.Controllers.Notifications;

[AppPermission(NotifoRoles.AppUser)]
public sealed class NotificationHub(
    IUserNotificationStore userNotificationsStore,
    IUserNotificationService userNotificationService)
    : Hub
{
    private static readonly UserNotificationQuery DefaultQuery = new UserNotificationQuery { Take = 100 };

    private string AppId => Context.User!.AppId()!;

    private string UserId => Context.User!.UserId()!;

    public override async Task OnConnectedAsync()
    {
        var notifications = await userNotificationsStore.QueryAsync(AppId, UserId, DefaultQuery, Context.ConnectionAborted);

        // Create the notifications with the correct channel (web).
        var dtos = notifications.Select(x => UserNotificationDto.FromDomainObject(x, Providers.Web)).ToArray();

        await Clients.Caller.SendAsync("notifications", dtos, Context.ConnectionAborted);
    }

    public async Task Delete(Guid id)
    {
        await userNotificationsStore.DeleteAsync(id, Context.ConnectionAborted);

        await Clients.User(Context.User?.Sub()!).SendAsync("notificationDeleted", new { id }, Context.ConnectionAborted);
    }

    public async Task ConfirmMany(TrackNotificationDto request)
    {
        if (request.Confirmed != null)
        {
            var token = ParseToken(request.Confirmed, request);

            await userNotificationService.TrackConfirmedAsync([token]);
        }

        if (request.Seen?.Length > 0)
        {
            var tokens = request.Seen.Select(x => ParseToken(x, request)).ToArray();

            await userNotificationService.TrackSeenAsync(tokens);
        }
    }

    private static TrackingToken ParseToken(string id, TrackNotificationDto request)
    {
        return TrackingToken.Parse(
            id,
            request.Channel,
            request.ConfigurationId,
            request.DeviceIdentifier);
    }
}

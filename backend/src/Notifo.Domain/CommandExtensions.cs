// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Identity;
using NodaTime;
using Notifo.Domain.Users;

namespace Notifo.Domain;

public static class CommandExtensions
{
    public static T WithTimestamp<T>(this T command, Instant timestamp) where T : CommandBase
    {
        command.Timestamp = timestamp;
        return command;
    }

    public static T WithTracking<T>(this T command, TrackingKey tracking) where T : CommandBase
    {
        return WithTracking(command, tracking.AppId, tracking.UserId);
    }

    public static T WithTracking<T>(this T command, string? appId, string? userId) where T : CommandBase
    {
        if (appId != null && command is AppCommandBase appCommand)
        {
            appCommand.AppId = appId;
        }

        if (userId != null)
        {
            command.Principal = CommandBase.BackendUser(userId);
            command.PrincipalId = userId;
        }

        if (userId != null && command is UserCommand userCommand)
        {
            userCommand.UserId = userId;
        }

        return command;
    }
}

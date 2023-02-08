// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using NodaTime;

namespace Notifo.Domain;

public static class CommandExtensions
{
    public static T WithTimestamp<T>(this T command, Instant timestamp) where T : CommandBase
    {
        command.Timestamp = timestamp;
        return command;
    }

    public static T With<T>(this T command, string? appId, string? userId) where T : CommandBase
    {
        if (appId != null && command is AppCommandBase appCommand)
        {
            appCommand.AppId = appId;
        }

        if (userId != null)
        {
            command.Principal = BackendUser(userId);
            command.PrincipalId = userId;
        }

        if (userId != null && command is UserCommandBase userCommand)
        {
            userCommand.UserId = userId;
        }

        return command;
    }

    private static ClaimsPrincipal BackendUser(string userId)
    {
        var claimsIdentity = new ClaimsIdentity();
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        claimsIdentity.AddClaim(new Claim("sub", userId));

        return claimsPrincipal;
    }
}

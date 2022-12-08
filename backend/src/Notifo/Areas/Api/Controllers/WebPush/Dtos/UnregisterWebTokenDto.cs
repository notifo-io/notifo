// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Users;

namespace Notifo.Areas.Api.Controllers.WebPush.Dtos;

public sealed class UnregisterWebTokenDto
{
    public string Endpoint { get; set; }

    public RemoveUserWebPushSubscription ToDelete(string userId)
    {
        var result = new RemoveUserWebPushSubscription
        {
            Endpoint = Endpoint,

            // User ID is coming from the route in this context.
            UserId = userId
        };

        return result;
    }
}

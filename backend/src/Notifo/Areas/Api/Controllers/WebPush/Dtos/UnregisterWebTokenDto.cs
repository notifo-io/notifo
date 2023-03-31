// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Build.Framework;
using Notifo.Areas.Api.OpenApi;
using Notifo.Domain.Users;

namespace Notifo.Areas.Api.Controllers.WebPush.Dtos;

[OpenApiRequest]
public sealed class UnregisterWebTokenDto
{
    [Required]
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

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
public sealed class RegisterWebTokenDto
{
    [Required]
    public RegisterWebTokenSubscriptionDto Subscription { get; set; }

    public AddUserWebPushSubscription ToUpdate(string userId)
    {
        var result = new AddUserWebPushSubscription
        {
            Subscription = Subscription.ToSubscription(),

            // User ID is coming from the route in this context.
            UserId = userId
        };

        return result;
    }
}

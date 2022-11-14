// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Users;

namespace Notifo.Areas.Api.Controllers.WebPush.Dtos;

public sealed class RegisterWebTokenDto
{
    public WebPushSubscriptionDto Subscription { get; set; }

    public AddUserWebPushSubscription ToUpdate(string userId)
    {
        var result = new AddUserWebPushSubscription
        {
            Subscription = Subscription.ToSubscription()
        };

        result.UserId = userId;

        return result;
    }

    public RemoveUserWebPushSubscription ToDelete(string userId)
    {
        var result = new RemoveUserWebPushSubscription
        {
            Endpoint = Subscription.Endpoint
        };

        result.UserId = userId;

        return result;
    }
}

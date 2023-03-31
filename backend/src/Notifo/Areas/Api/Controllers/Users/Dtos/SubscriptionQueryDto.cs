// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Areas.Api.OpenApi;
using Notifo.Domain.Subscriptions;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.Users.Dtos;

[OpenApiRequest]
public sealed class SubscriptionQueryDto : QueryDto
{
    /// <summary>
    /// The topics we are interested in.
    /// </summary>
    public string? Topics { get; set; }

    public SubscriptionQuery ToQuery(bool needsTotal, string? userId)
    {
        var result = SimpleMapper.Map(this, new SubscriptionQuery
        {
            TotalNeeded = needsTotal
        });

        result.UserId = userId;

        if (!string.IsNullOrEmpty(Topics))
        {
            result.Topics = Topics.Split(',');
        }

        return result;
    }
}

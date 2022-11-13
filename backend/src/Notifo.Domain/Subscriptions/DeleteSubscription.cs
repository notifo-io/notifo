// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;

namespace Notifo.Domain.Subscriptions;

public sealed class DeleteSubscription : SubscriptionCommand
{
    public override bool IsUpsert => false;

    public override async ValueTask ExecuteAsync(IServiceProvider serviceProvider,
        CancellationToken ct)
    {
        await serviceProvider.GetRequiredService<ISubscriptionRepository>()
            .DeleteAsync(AppId, UserId, Topic, ct);
    }
}

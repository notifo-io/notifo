// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Subscriptions;
using Notifo.Domain.Subscriptions.MongoDb;

namespace Microsoft.Extensions.DependencyInjection;

public static class SubscriptionsServiceExtensions
{
    public static void AddMySubscriptions(this IServiceCollection services)
    {
        services.AddSingletonAs<SubscriptionStore>()
            .As<ISubscriptionStore>();

        services.AddRequestHandler<SubscriptionStore, SubscriptionCommand, Subscription?>();
    }

    public static void AddMyMongoSubscriptions(this IServiceCollection services)
    {
        services.AddSingletonAs<MongoDbSubscriptionRepository>()
            .As<ISubscriptionRepository>();
    }
}

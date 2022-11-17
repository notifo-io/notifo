// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.UserNotifications.MongoDb;

namespace Notifo.Domain.Subscriptions.MongoDb;

public sealed class MongoDbSubscriptionRepositoryFixture : MongoFixtureBase
{
    public MongoDbSubscriptionRepository Repository { get; }

    public MongoDbSubscriptionRepositoryFixture()
    {
        Repository = new MongoDbSubscriptionRepository(MongoDatabase);
        Repository.InitializeAsync(default).Wait();
    }
}

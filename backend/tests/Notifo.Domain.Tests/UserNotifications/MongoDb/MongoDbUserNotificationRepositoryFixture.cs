// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Notifo.Domain.UserNotifications.MongoDb;

public sealed class MongoDbUserNotificationRepositoryFixture : MongoFixtureBase, IDisposable
{
    public MongoDbUserNotificationRepository Repository { get; }

    public MongoDbUserNotificationRepositoryFixture()
    {
        var options = new UserNotificationsOptions
        {
            MaxItemsPerUser = 100
        };

        var log = A.Fake<ILogger<MongoDbUserNotificationRepository>>();

        Repository = new MongoDbUserNotificationRepository(MongoDatabase, Options.Create(options), log);
        Repository.InitializeAsync(default).Wait();
    }
}

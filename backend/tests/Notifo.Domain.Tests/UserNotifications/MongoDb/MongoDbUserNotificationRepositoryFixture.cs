// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Notifo.Infrastructure.MongoDb;

namespace Notifo.Domain.UserNotifications.MongoDb;

public sealed class MongoDbUserNotificationRepositoryFixture : IDisposable
{
    public MongoDbUserNotificationRepository Repository { get; }

    public MongoDbUserNotificationRepositoryFixture()
    {
        ActivityContextSerializer.Register();
        ActivitySpanIdSerializer.Register();
        ActivityTraceIdSerializer.Register();

        InstantSerializer.Register();

        var mongoClient = new MongoClient("mongodb://localhost");
        var mongoDatabase = mongoClient.GetDatabase("Notifo_Testing");

        var options = new UserNotificationsOptions
        {
            MaxItemsPerUser = 100
        };

        var log = A.Fake<ILogger<MongoDbUserNotificationRepository>>();

        Repository = new MongoDbUserNotificationRepository(mongoDatabase, Options.Create(options), log);
        Repository.InitializeAsync(default).Wait();
    }

    public void Dispose()
    {
    }
}

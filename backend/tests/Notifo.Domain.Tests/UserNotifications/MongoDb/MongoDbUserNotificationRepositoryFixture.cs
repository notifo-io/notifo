// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using EphemeralMongo;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Notifo.Infrastructure.MongoDb;
using System.Diagnostics;

namespace Notifo.Domain.UserNotifications.MongoDb;

public sealed class MongoDbUserNotificationRepositoryFixture : IDisposable
{
    private readonly IMongoRunner? runner;

    public MongoDbUserNotificationRepository Repository { get; }

    public MongoDbUserNotificationRepositoryFixture()
    {
        ActivityContextSerializer.Register();
        ActivitySpanIdSerializer.Register();
        ActivityTraceIdSerializer.Register();

        InstantSerializer.Register();

        IMongoClient mongoClient;

        if (Debugger.IsAttached)
        {
            mongoClient = new MongoClient("mongodb://localhost");
        }
        else
        {
            runner = MongoRunner.Run();

            mongoClient = new MongoClient(runner.ConnectionString);
        }

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
        runner?.Dispose();
    }
}

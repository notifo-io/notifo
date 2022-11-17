// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;
using EphemeralMongo;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Notifo.Infrastructure.MongoDb;

namespace Notifo.Domain.UserNotifications.MongoDb;

public sealed class MongoDbUserNotificationRepositoryFixture : IDisposable
{
    private readonly IMongoRunner? runner;

    public MongoDbUserNotificationRepository Repository { get; }

    public IMongoClient MongoClient { get; }

    public IMongoDatabase MongoDatabase { get; }

    public MongoDbUserNotificationRepositoryFixture()
    {
        ActivityContextSerializer.Register();
        ActivitySpanIdSerializer.Register();
        ActivityTraceIdSerializer.Register();

        InstantSerializer.Register();

        if (Debugger.IsAttached)
        {
            MongoClient = new MongoClient("mongodb://localhost");
        }
        else
        {
            runner = MongoRunner.Run();

            MongoClient = new MongoClient(runner.ConnectionString);
        }

        MongoDatabase = MongoClient.GetDatabase("Notifo_Testing");

        var options = new UserNotificationsOptions
        {
            MaxItemsPerUser = 100
        };

        var log = A.Fake<ILogger<MongoDbUserNotificationRepository>>();

        Repository = new MongoDbUserNotificationRepository(MongoDatabase, Options.Create(options), log);
        Repository.InitializeAsync(default).Wait();
    }

    public void Dispose()
    {
        runner?.Dispose();
    }
}

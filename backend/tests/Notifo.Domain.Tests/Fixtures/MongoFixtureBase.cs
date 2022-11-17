// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;
using EphemeralMongo;
using MongoDB.Driver;
using Notifo.Infrastructure.MongoDb;

namespace Notifo.Domain.UserNotifications.MongoDb;

public abstract class MongoFixtureBase : IDisposable
{
    private readonly IMongoRunner? runner;

    public IMongoClient MongoClient { get; }

    public IMongoDatabase MongoDatabase { get; }

    protected MongoFixtureBase()
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
    }

    public void Dispose()
    {
        runner?.Dispose();
    }
}

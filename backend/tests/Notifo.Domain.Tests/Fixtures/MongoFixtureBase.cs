// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;
using EphemeralMongo;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
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
        BsonSerializer.RegisterSerializer(new ObjectSerializer(type => true));

        if (Debugger.IsAttached)
        {
            MongoClient = new MongoClient("mongodb://localhost");
        }
        else
        {
            runner = MongoRunnerProvider.Get();

            MongoClient = new MongoClient(runner.ConnectionString);
        }

        MongoDatabase = MongoClient.GetDatabase("Notifo_Testing");
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        runner?.Dispose();
    }
}

#pragma warning disable MA0048 // File name must match type name
public static class MongoRunnerProvider
#pragma warning restore MA0048 // File name must match type name
{
    private static readonly object LockObject = new object();
    private static IMongoRunner? runner;
    private static int useCounter;

    public static IMongoRunner Get()
    {
        lock (LockObject)
        {
            runner ??= MongoRunner.Run();

            useCounter++;

            return new MongoRunnerWrapper(runner);
        }
    }

    private sealed class MongoRunnerWrapper : IMongoRunner
    {
        private IMongoRunner? underlyingMongoRunner;

        public string ConnectionString
        {
            get => GetRunner().ConnectionString;
        }

        public MongoRunnerWrapper(IMongoRunner underlyingMongoRunner)
        {
            this.underlyingMongoRunner = underlyingMongoRunner;
        }

        public void Import(string database, string collection, string inputFilePath, string? additionalArguments = null, bool drop = false)
        {
            GetRunner().Import(database, collection, inputFilePath, additionalArguments, drop);
        }

        public void Export(string database, string collection, string outputFilePath, string? additionalArguments = null)
        {
            GetRunner().Export(database, collection, outputFilePath, additionalArguments);
        }

        private IMongoRunner GetRunner()
        {
            if (underlyingMongoRunner == null)
            {
                throw new ObjectDisposedException(nameof(IMongoRunner));
            }

            return underlyingMongoRunner;
        }

        public void Dispose()
        {
            if (underlyingMongoRunner != null)
            {
                underlyingMongoRunner = null;
                StaticDispose();
            }
        }

        private static void StaticDispose()
        {
            lock (LockObject)
            {
                if (runner != null)
                {
                    useCounter--;
                    if (useCounter == 0)
                    {
                        runner.Dispose();
                        runner = null;
                    }
                }
            }
        }
    }
}

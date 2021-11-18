// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;
using Notifo.Infrastructure.Scheduling;
using Notifo.Infrastructure.Scheduling.Implementation.TimerBased.MongoDb;

namespace Notifo.Infrastructure.MongoDb.Scheduling
{
    public sealed class MongoDbSchedulerStoreFixture : IDisposable
    {
        public MongoDbSchedulerStore<int> Store { get; }

        public MongoDbSchedulerStoreFixture()
        {
            InstantSerializer.Register();

            var mongoClient = new MongoClient("mongodb://localhost");
            var mongoDatabase = mongoClient.GetDatabase("Testing");

            Store = new MongoDbSchedulerStore<int>(mongoDatabase, new SchedulerOptions { QueueName = "Numbers" });
            Store.InitializeAsync(default).Wait();
            Store.ClearAsync().Wait();
        }

        public void Dispose()
        {
        }
    }
}

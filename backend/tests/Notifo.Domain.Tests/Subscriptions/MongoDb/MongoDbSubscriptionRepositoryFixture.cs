// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using MongoDB.Driver;

namespace Notifo.Domain.Subscriptions.MongoDb
{
    public sealed class MongoDbSubscriptionRepositoryFixture : IDisposable
    {
        public MongoDbSubscriptionRepository Repository { get; }

        public MongoDbSubscriptionRepositoryFixture()
        {
            var mongoClient = new MongoClient("mongodb://localhost");
            var mongoDatabase = mongoClient.GetDatabase("Notifo_Testing");

            Repository = new MongoDbSubscriptionRepository(mongoDatabase);
            Repository.InitializeAsync(default).Wait();
        }

        public void Dispose()
        {
        }
    }
}

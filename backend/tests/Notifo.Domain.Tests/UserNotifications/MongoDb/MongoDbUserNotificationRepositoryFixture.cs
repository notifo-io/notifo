// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Notifo.Infrastructure.MongoDb;
using Squidex.Log;

namespace Notifo.Domain.UserNotifications.MongoDb
{
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

            Repository = new MongoDbUserNotificationRepository(mongoDatabase, A.Fake<ISemanticLog>(), Options.Create(options));
            Repository.InitializeAsync(default).Wait();
        }

        public void Dispose()
        {
        }
    }
}

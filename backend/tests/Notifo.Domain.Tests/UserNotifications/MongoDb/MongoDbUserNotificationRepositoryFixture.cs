// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Notifo.Infrastructure.MongoDb;

namespace Notifo.Domain.UserNotifications.MongoDb
{
    public sealed class MongoDbUserNotificationRepositoryFixture : IDisposable
    {
        public MongoDbUserNotificationRepository Repository { get; }

        public MongoDbUserNotificationRepositoryFixture()
        {
            InstantSerializer.Register();

            var mongoClient = new MongoClient("mongodb://localhost");
            var mongoDatabase = mongoClient.GetDatabase("Notifo_Testing");

            Repository = new MongoDbUserNotificationRepository(mongoDatabase, Options.Create(new UserNotificationsOptions()));
            Repository.InitializeAsync(default).Wait();
        }

        public void Dispose()
        {
        }
    }
}

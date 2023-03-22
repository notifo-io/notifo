// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization;
using Notifo.Domain;
using Notifo.Infrastructure.Collections;
using Notifo.Infrastructure.Collections.Bson;
using Notifo.Infrastructure.MongoDb;
using Notifo.Pipeline;
using Squidex.Messaging.Implementation.Null;
using Squidex.Messaging.Redis;
using StackExchange.Redis;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceExtensions
{
    private sealed class RedisConnection
    {
        private readonly string connectionString;
        private Task<IConnectionMultiplexer> connection;

        public RedisConnection(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public Task<IConnectionMultiplexer> ConnectAsync(TextWriter writer)
        {
            return connection ??= ConnectCoreAsync(writer);
        }

        public async Task<IConnectionMultiplexer> ConnectCoreAsync(TextWriter writer)
        {
            return await ConnectionMultiplexer.ConnectAsync(connectionString, writer);
        }
    }

    public static void AddMyStorage(this IServiceCollection services, IConfiguration config)
    {
        config.ConfigureByOption("storage:type", new Alternatives
        {
            ["MongoDB"] = () =>
            {
                SoftEnumSerializer<ConfirmMode>.Register();

                services.AddMyMongoApps();
                services.AddMyMongoDb(config);
                services.AddMyMongoDbIdentity();
                services.AddMyMongoDbKeyValueStore();
                services.AddMyMongoDbScheduler();
                services.AddMyMongoEvents();
                services.AddMyMongoLog();
                services.AddMyMongoMedia();
                services.AddMyMongoSubscriptions();
                services.AddMyMongoTemplates();
                services.AddMyMongoTopics();
                services.AddMyMongoUserNotifications();
                services.AddMyMongoUsers();
            }
        });
    }

    public static void AddMyClustering(this IServiceCollection services, IConfiguration config, SignalROptions signalROptions)
    {
        config.ConfigureByOption("clustering:type", new Alternatives
        {
            ["Redis"] = () =>
            {
                var connection = new RedisConnection(config.GetRequiredValue("clustering:redis:connectionString"));

                if (signalROptions.Enabled)
                {
                    services.AddSignalR()
                        .AddStackExchangeRedis(options =>
                        {
                            options.ConnectionFactory = connection.ConnectAsync;
                        });
                }

                services.AddRedisTransport(config, options =>
                {
                    options.ConnectionFactory = connection.ConnectAsync;
                });

                services.AddReplicatedCacheMessaging(true, options =>
                {
                    options.TransportSelector = (transports, name) => transports.First(x => x is RedisTransport);
                });
            },
            ["None"] = () =>
            {
                services.AddReplicatedCacheMessaging(false, options =>
                {
                    options.TransportSelector = (transports, name) => transports.First(x => x is NullTransport);
                });
            }
        });
    }
}

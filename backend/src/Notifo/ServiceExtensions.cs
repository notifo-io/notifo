// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Authentication;
using Notifo.Domain;
using Notifo.Infrastructure.MongoDb;
using Notifo.Pipeline;
using Squidex.Messaging.Implementation.Null;
using Squidex.Messaging.Redis;
using StackExchange.Redis;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceExtensions
{
    private sealed class RedisConnection(string connectionString)
    {
        private Task<IConnectionMultiplexer> connection;

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

    public static void AddMyMessaging(this IServiceCollection services, IConfiguration config)
    {
        var builder = services.AddMessaging()
            .AddMyUserEvents(config)
            .AddMyUserNotifications(config);

#if INCLUDE_KAFKA
        var type = config.GetValue<string>("messaging:type");

        if (string.Equals(type, "Kafka", StringComparison.OrdinalIgnoreCase))
        {
            builder.AddKafkaTransport(config);
            return;
        }
#endif

        builder.AddTransport(config);
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

                services.AddMessaging()
                    .AddRedisTransport(config, options =>
                    {
                        options.ConnectionFactory = connection.ConnectAsync;
                    })
                    .AddReplicatedCache(true, options =>
                    {
                        options.TransportSelector = (transports, name) => transports.First(x => x is RedisTransport);
                    });
            },
            ["None"] = () =>
            {
                services.AddMessaging()
                    .AddReplicatedCache(false, options =>
                    {
                        options.TransportSelector = (transports, name) => transports.First(x => x is NullTransport);
                    });
            }
        });
    }
}

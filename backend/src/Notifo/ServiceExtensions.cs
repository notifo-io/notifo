// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Notifo.Pipeline;
using StackExchange.Redis;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceExtensions
    {
        private sealed class RedisConnection
        {
            private readonly SemaphoreSlim connectionLock = new SemaphoreSlim(1);
            private readonly string connectionString;
            private IConnectionMultiplexer connection;

            public RedisConnection(string connectionString)
            {
                this.connectionString = connectionString;
            }

            public async Task<IConnectionMultiplexer> ConnectAsync(TextWriter writer)
            {
                if (connection == null)
                {
                    await connectionLock.WaitAsync();
                    try
                    {
                        connection ??= await ConnectionMultiplexer.ConnectAsync(connectionString, writer);
                    }
                    finally
                    {
                        connectionLock.Release();
                    }
                }

                return connection;
            }
        }

        public static void AddMyStorage(this IServiceCollection services, IConfiguration config)
        {
            config.ConfigureByOption("storage:type", new Alternatives
            {
                ["MongoDB"] = () =>
                {
                    services.AddMyMongoDb(config);
                    services.AddMyMongoApps();
                    services.AddMyMongoDbIdentity();
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

                    services.AddRedisPubSub(options =>
                    {
                        options.ConnectionFactory = connection.ConnectAsync;
                    });
                },
                ["None"] = () =>
                {
                    // NOOP
                }
            });
        }
    }
}

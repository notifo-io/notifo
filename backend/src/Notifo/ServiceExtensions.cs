// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Authentication;
using Notifo.Pipeline;
using Squidex.Hosting.Configuration;
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
                services.AddMyMongoDbStore(config);
            },
            ["Sql"] = () =>
            {
                services.AddMyEntityFrameworkStore(config);
            }
        });
    }

    public static void AddMyAssetStore(this IServiceCollection services, IConfiguration config)
    {
        config.ConfigureByOption("assetStore:type", new Alternatives
        {
            ["Folder"] = () =>
            {
                services.AddFolderAssetStore(config);
            },
            ["FTP"] = () =>
            {
                services.AddFTPAssetStore(config);
            },
            ["GoogleCloud"] = () =>
            {
                services.AddGoogleCloudAssetStore(config);
            },
            ["AzureBlob"] = () =>
            {
                services.AddAzureBlobAssetStore(config);
            },
            ["AmazonS3"] = () =>
            {
                services.AddAmazonS3AssetStore(config);
            },
            ["MongoDb"] = () =>
            {
                if (IsSqlStorage(config))
                {
                    throw new ConfigurationException(
                        new ConfigurationError(
                            "MongoDb asset store is only allowed, when 'storage:type' is set to 'MongoDB'.",
                            "assetStore:type"));
                }

                services.AddMyMongoDbAssetStore(config);
            }
        });
    }

    public static void AddMyMessaging(this IServiceCollection services, IConfiguration config)
    {
        var builder = services.AddMessaging()
            .AddMyUserEvents(config)
            .AddMyUserNotifications(config);

        var type = config.GetValue<string>("messaging:type");
        if (string.Equals(type, "Sql", StringComparison.OrdinalIgnoreCase))
        {
            if (!IsSqlStorage(config))
            {
                throw new ConfigurationException(
                    new ConfigurationError(
                        "Sql messaging transport is only allowed, when 'storage:type' is also set to 'Sql'.",
                        "messaging:type"));
            }

            builder.AddMyEntityFrameworkTransport(config);
            return;
        }

#if INCLUDE_KAFKA
        if (string.Equals(type, "Kafka", StringComparison.OrdinalIgnoreCase))
        {
            builder.AddKafkaTransport(config);
            return;
        }
#endif

        // The scheduler transport stores the messages in the MongoDB database of the storage.
        if (string.Equals(type, "Scheduler", StringComparison.OrdinalIgnoreCase) && IsSqlStorage(config))
        {
            throw new ConfigurationException(
                new ConfigurationError(
                    "Scheduler messaging transport is only allowed, when 'storage:type' is set to 'MongoDB'. Use 'Sql' instead.",
                    "messaging:type"));
        }

        builder.AddTransport(config);
    }

    private static bool IsSqlStorage(IConfiguration config)
    {
        return string.Equals(config.GetValue<string>("storage:type"), "Sql", StringComparison.OrdinalIgnoreCase);
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

// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.Configuration;
using Notifo;
using Notifo.Pipeline;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
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

        public static void AddMyTelemetry(this IServiceCollection services, IConfiguration config)
        {
            services.AddOpenTelemetryTracing(builder =>
            {
                builder.SetResourceBuilder(
                    ResourceBuilder.CreateDefault()
                        .AddService("Notifo",
                            "Notifo",
                            typeof(Startup).Assembly.GetName().Version!.ToString()));

                builder.AddSource("Notifo");

                builder.AddAspNetCoreInstrumentation();
                builder.AddHttpClientInstrumentation();

                if (config.GetValue<bool>("logging:stackdriver:enabled"))
                {
                    var projectId = config.GetRequiredValue("logging:stackdriver:projectId");

                    builder.UseStackdriverExporter(projectId);
                }

                if (config.GetValue<bool>("logging:applicationInsights:enabled"))
                {
                    builder.AddAzureMonitorTraceExporter(options =>
                    {
                        config.GetSection("logging:applicationInsights").Bind(options);
                    });
                }

                if (config.GetValue<bool>("logging:otlp:enabled"))
                {
                    // See: https://docs.microsoft.com/aspnet/core/grpc/troubleshoot#call-insecure-grpc-services-with-net-core-client
                    AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

                    builder.AddOtlpExporter(options =>
                    {
                        config.GetSection("logging:otlp").Bind(options);
                    });
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

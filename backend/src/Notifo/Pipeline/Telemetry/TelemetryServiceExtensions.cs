// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Reflection;
using Azure.Monitor.OpenTelemetry.Exporter;
using Google.Cloud.Diagnostics.Common;
using Notifo;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class TelemetryServiceExtensions
    {
        public static void AddMyTelemetry(this IServiceCollection services, IConfiguration config)
        {
            services.AddOpenTelemetryTracing(builder =>
            {
                var serviceName = config.GetValue<string>("logging:name") ?? "Notifo";
                var serviceVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString();

                builder.SetResourceBuilder(
                    ResourceBuilder.CreateDefault()
                        .AddService(serviceName,
                            "Notifo",
                            typeof(Startup).Assembly.GetName().Version!.ToString()));

                builder.AddSource("Notifo");

                builder.AddAspNetCoreInstrumentation();
                builder.AddHttpClientInstrumentation();
                builder.AddMongoDBInstrumentation();

                if (config.GetValue<bool>("logging:stackdriver:enabled"))
                {
                    var projectId = config.GetRequiredValue("logging:stackdriver:projectId");

                    builder.UseStackdriverExporter(projectId);

                    services.AddSingleton(c => ContextExceptionLogger.Create(projectId, serviceVersion, serviceVersion, null));
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
    }
}

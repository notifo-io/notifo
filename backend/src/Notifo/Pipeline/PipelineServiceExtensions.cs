// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Net.Http.Headers;
using Notifo.Domain;
using Notifo.Infrastructure.Diagnostics;
using Notifo.Infrastructure.Json;

namespace Notifo.Pipeline;

public static class PipelineServiceExtensions
{
    public static void AddMyWebPipeline(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<DiagnoserOptions>(config, "diagnostics");
        services.Configure<GCHealthCheckOptions>(config, "diagnostics:gc");

        services.AddHealthChecks()
            .AddCheck<GCHealthCheck>("gc");

        services.AddSingletonAs<Diagnoser>()
            .AsSelf();

        services.AddSingletonAs<FileCallbackResultExecutor>()
            .AsSelf();
    }

    public static void AddPipeline(this IServiceCollection services)
    {
        services.AddSingleton<AppMediatorMiddleware>();
        services.AddMiddleware<AppMediatorMiddleware, AppCommandBase>();
    }

    public static IApplicationBuilder UseMyHealthChecks(this IApplicationBuilder app)
    {
        var serializer = app.ApplicationServices.GetRequiredService<IJsonSerializer>();

        var writer = new Func<HttpContext, HealthReport, Task>((httpContext, report) =>
        {
            var response = new
            {
                Entries = report.Entries.ToDictionary(x => x.Key, x =>
                {
                    var value = x.Value;

                    return new
                    {
                        Data = value.Data.Count > 0 ? new Dictionary<string, object>(value.Data) : null,
                        value.Description,
                        value.Duration,
                        value.Status
                    };
                }),
                report.Status,
                report.TotalDuration
            };

            var json = serializer.SerializeToString(response);

            httpContext.Response.Headers[HeaderNames.ContentType] = "text/json";

            return httpContext.Response.WriteAsync(json, httpContext.RequestAborted);
        });

        app.UseHealthChecks("/healthz", new HealthCheckOptions
        {
            ResponseWriter = writer
        });

        return app;
    }
}

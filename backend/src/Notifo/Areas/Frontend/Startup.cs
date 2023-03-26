// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.FileProviders;
using Microsoft.Net.Http.Headers;
using Notifo.Pipeline;
using Squidex.Hosting.Web;

namespace Notifo.Areas.Frontend;

public static class Startup
{
    public static void UseFrontend(this IApplicationBuilder app)
    {
        var environment = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();

        var fileProvider = environment.WebRootFileProvider;

        if (environment.IsProduction())
        {
            fileProvider = new CompositeFileProvider(fileProvider,
                new PhysicalFileProvider(Path.Combine(environment.WebRootPath, "build")));
        }

        app.UseMiddleware<NotifoMiddleware>();
        app.UseMiddleware<SetupMiddleware>();

        app.UseHtmlTransform(new HtmlTransformOptions
        {
            Transform = (html, context) =>
            {
                return new ValueTask<string>(html.AddOptions(context));
            }
        });

        app.UseNotifoStaticFiles(fileProvider);

        if (environment.IsProduction())
        {
            // Try static files again to serve index.html.
            app.UsePathOverride("/index.html");
            app.UseNotifoStaticFiles(fileProvider);
        }
        else
        {
            // Forward requests to SPA development server.
            app.UseSpa(builder =>
            {
                builder.UseProxyToSpaDevelopmentServer("https://localhost:3002");
            });
        }
    }

    private static void UseNotifoStaticFiles(this IApplicationBuilder app, IFileProvider fileProvider)
    {
        app.UseStaticFiles(new StaticFileOptions
        {
            OnPrepareResponse = context =>
            {
                var response = context.Context.Response;

                if (!string.IsNullOrWhiteSpace(context.Context.Request.QueryString.ToString()))
                {
                    response.Headers[HeaderNames.CacheControl] = "max-age=5184000";
                }
                else if (string.Equals(response.ContentType, "text/html", StringComparison.OrdinalIgnoreCase))
                {
                    response.Headers[HeaderNames.CacheControl] = "no-cache";
                }
            },
            FileProvider = fileProvider
        });
    }
}

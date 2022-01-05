// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.FileProviders;
using Microsoft.Net.Http.Headers;
using Notifo.Pipeline;

namespace Notifo.Areas.Frontend
{
    public static class Startup
    {
        public static void ConfigureFrontend(this IApplicationBuilder app)
        {
            var environment = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();

            app.UseMiddleware<NotifoMiddleware>();
            app.UseHtmlTransform();

            var fileProvider = environment.WebRootFileProvider;

            if (environment.IsProduction())
            {
                fileProvider = new CompositeFileProvider(fileProvider,
                    new PhysicalFileProvider(Path.Combine(environment.WebRootPath, "build")));
            }

            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = context =>
                {
                    var request = context.Context.Request;

                    var hasQuery = !string.IsNullOrWhiteSpace(request.QueryString.ToString());

                    var response = context.Context.Response;
                    var responseHeaders = response.GetTypedHeaders();

                    if (hasQuery)
                    {
                        responseHeaders.CacheControl = new CacheControlHeaderValue
                        {
                            MaxAge = TimeSpan.FromDays(60)
                        };
                    }
                    else if (string.Equals(response.ContentType, "text/html", StringComparison.OrdinalIgnoreCase))
                    {
                        responseHeaders.CacheControl = new CacheControlHeaderValue
                        {
                            NoCache = true
                        };
                    }
                },
                FileProvider = fileProvider
            });

            if (environment.IsDevelopment())
            {
                app.UseSpa(builder =>
                {
                    builder.UseProxyToSpaDevelopmentServer("https://localhost:3002");
                });
            }
        }
    }
}

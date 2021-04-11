// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using Notifo.Areas.Frontend.Middlewares;
using Notifo.Pipeline;

namespace Notifo.Areas.Frontend
{
    public static class Startup
    {
        public static void ConfigureFrontend(this IApplicationBuilder app)
        {
            var environment = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();

            app.UseMiddleware<NotifoMiddleware>();

            app.Use((context, next) =>
            {
                if (!Path.HasExtension(context.Request.Path.Value))
                {
                    if (environment.IsDevelopment())
                    {
                        context.Request.Path = new PathString("/index.html");
                    }
                    else
                    {
                        context.Request.Path = new PathString("/build/index.html");
                    }
                }

                if (environment.IsProduction() && context.Request.Path.Equals("/demo.html"))
                {
                    context.Request.Path = new PathString("/build/demo.html");
                }

                return next();
            });

            if (environment.IsDevelopment())
            {
                app.UseMiddleware<WebpackMiddleware>();
            }
            else
            {
                app.UseMiddleware<IndexMiddleware>();
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
                }
            });
        }
    }
}

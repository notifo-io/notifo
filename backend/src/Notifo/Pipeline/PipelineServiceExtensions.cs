// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Notifo.Areas.Api.Utils;

namespace Notifo.Pipeline
{
    public static class PipelineServiceExtensions
    {
        public static void AddMyWebPipeline(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingletonAs<FileCallbackResultExecutor>()
                .AsSelf();

            var urlsOptions = config.GetSection("url").Get<UrlOptions>();

            var host = urlsOptions.BuildHost();

            if (urlsOptions.EnforceHost)
            {
                services.AddHostFiltering(options =>
                {
                    options.AllowEmptyHosts = true;
                    options.AllowedHosts.Add(host.Host);

                    options.IncludeFailureMessage = false;
                });
            }

            if (urlsOptions.EnforceHTTPS && !string.Equals(host.Host, "localhost", StringComparison.OrdinalIgnoreCase))
            {
                services.AddHttpsRedirection(options =>
                {
                    options.HttpsPort = urlsOptions.HttpsPort;
                });
            }
        }

        public static void UseMyForwardingRules(this IApplicationBuilder app)
        {
            var urlsOptions = app.ApplicationServices.GetRequiredService<IOptions<UrlOptions>>().Value;

            if (urlsOptions.EnableForwardHeaders)
            {
                var options = new ForwardedHeadersOptions
                {
                    AllowedHosts = new List<string>
                    {
                        new Uri(urlsOptions.BaseUrl).Host
                    },
                    ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost,
                    ForwardLimit = null,
                    RequireHeaderSymmetry = false
                };

                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();

                if (urlsOptions.KnownProxies != null)
                {
                    foreach (var proxy in urlsOptions.KnownProxies)
                    {
                        if (IPAddress.TryParse(proxy, out var address))
                        {
                            options.KnownProxies.Add(address);
                        }
                    }
                }

                app.UseForwardedHeaders(options);
            }

            app.UseMiddleware<CleanupHostMiddleware>();

            if (urlsOptions.EnforceHost)
            {
                app.UseHostFiltering();
            }

            if (urlsOptions.EnforceHTTPS)
            {
                app.UseHttpsRedirection();
            }
        }
    }
}

// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notifo.Areas.Api;
using Notifo.Areas.Frontend;
using Notifo.Domain.Counters;
using Notifo.Domain.Topics;
using Notifo.Pipeline;

namespace Notifo
{
    public class Startup
    {
        private readonly IConfiguration config;

        public Startup(IConfiguration config)
        {
            this.config = config;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDefaultWebServices(config);
            services.AddDefaultForwardRules();

            services.AddCors();

            services.AddLocalization(options =>
            {
                options.ResourcesPath = "Resources";
            });

            services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
            });

            services.AddSignalR()
                .AddJsonProtocol(options =>
                {
                    options.PayloadSerializerOptions.Configure();
                });

            services.Configure<ApiBehaviorOptions>(options =>
                {
                    options.SuppressModelStateInvalidFilter = true;
                });

            services.AddHttpContextAccessor();
            services.AddMvc(options =>
                {
                    options.Filters.Add<AppResolver>();
                })
                .AddViewLocalization(options =>
                {
                    options.ResourcesPath = "Resources";
                })
                .AddDataAnnotationsLocalization(options =>
                {
                    options.DataAnnotationLocalizerProvider = (type, factory) => factory.Create(typeof(AppResources));
                })
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Configure();
                })
                .AddRazorRuntimeCompilation();

            services.AddMyApi();
            services.AddMyApps();
            services.AddMyAssets(config);
            services.AddMyCaching();
            services.AddMyCounters();
            services.AddMyClustering(config);
            services.AddMyEmailChannel(config);
            services.AddMyEvents(config);
            services.AddMyIdentity(config);
            services.AddMyJson();
            services.AddMyLog();
            services.AddMyMedia();
            services.AddMyMessaging(config);
            services.AddMyMobilePushChannel();
            services.AddMyNodaTime();
            services.AddMyOpenApi();
            services.AddMySmsChannel(config);
            services.AddMyStorage(config);
            services.AddMySubscriptions();
            services.AddMyTemplates();
            services.AddMyTopics();
            services.AddMyUserEvents(config);
            services.AddMyUserNotifications();
            services.AddMyUsers();
            services.AddMyUtils(config);
            services.AddMyWebChannel();
            services.AddMyWebhookChannel();
            services.AddMyWebPipeline();
            services.AddMyWebPushChannel(config);

            services.AddInitializer();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDefaultForwardRules();

            var cultures = new[]
            {
                new CultureInfo("en"),
                new CultureInfo("de")
            };

            app.UseRequestLocalization(options =>
            {
                options.DefaultRequestCulture = new RequestCulture(cultures[0]);
                options.SupportedCultures = cultures;
                options.SupportedUICultures = cultures;
            });

            app.UseMyHealthChecks();

            app.ConfigureApi();
            app.ConfigureFrontend();
        }
    }
}

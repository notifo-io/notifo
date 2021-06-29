// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Notifo.Areas.Api;
using Notifo.Areas.Frontend;
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
            var signalROptions = config.GetSection("web:signalR").Get<SignalROptions>();

            services.ConfigureAndValidate<SignalROptions>(config, "web:signalR");

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

            if (signalROptions.Enabled)
            {
                services.AddSignalR()
                    .AddJsonProtocol(options =>
                    {
                        options.PayloadSerializerOptions.Configure();
                    });
            }

            services.Configure<ApiBehaviorOptions>(options =>
                {
                    options.SuppressModelStateInvalidFilter = true;
                });

            services.AddHttpClient();
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

            services.AddMyApi(signalROptions);
            services.AddMyApps();
            services.AddMyAssets(config);
            services.AddMyCaching();
            services.AddMyCounters();
            services.AddMyClustering(config, signalROptions);
            services.AddMyEmailChannel();
            services.AddMyEvents(config);
            services.AddMyIdentity(config);
            services.AddMyIntegrations();
            services.AddMyJson();
            services.AddMyLog();
            services.AddMyMedia();
            services.AddMyMessaging(config);
            services.AddMyMessagingChannel();
            services.AddMyMobilePushChannel();
            services.AddMyNodaTime();
            services.AddMyOpenApi();
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

            services.IntegrateAmazonSES(config);
            services.IntegrateFirebase();
            services.IntegrateMailchimp();
            services.IntegrateMailjet();
            services.IntegrateMessageBird(config);
            services.IntegrateSmtp();
            services.IntegrateThreema();

            services.AddInitializer();
        }

        public void Configure(IApplicationBuilder app)
        {
            var signalROptions = app.ApplicationServices.GetRequiredService<IOptions<SignalROptions>>().Value;

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

            app.UseWhen(
                context => context.Request.Path.StartsWithSegments("/account"),
                builder =>
                {
                    builder.UseExceptionHandler("/account/error");
                });

            app.ConfigureApi(signalROptions);
            app.ConfigureFrontend();
        }
    }
}

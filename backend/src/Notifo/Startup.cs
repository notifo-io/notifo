// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Notifo.Areas.Api.Controllers.Notifications;
using Notifo.Areas.Frontend;
using Notifo.Domain;
using Notifo.Domain.Utils;
using Notifo.Pipeline;

namespace Notifo;

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
                    options.PayloadSerializerOptions.Configure(ConfigureJson);
                });
        }

        services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

        services.AddHttpClient();
        services.AddHttpContextAccessor();

        services.AddHttpClient("Unsafe")
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
            });

        services.AddRouting(options =>
        {
            options.ConstraintMap.Add("notEmpty", typeof(NotEmptyRouteConstraint));
        });

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
                options.JsonSerializerOptions.Configure(ConfigureJson);

                // Do not write null values.
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

                // Expose the json exceptions to the model state.
                options.AllowInputFormatterExceptionMessages = false;
            })
            .AddRazorRuntimeCompilation();

        services.AddMyApi(signalROptions);
        services.AddMyApiKey();
        services.AddMyApps();
        services.AddMyAssets(config);
        services.AddMyCaching();
        services.AddMyClustering(config, signalROptions);
        services.AddMyCounters();
        services.AddMyEmailChannel();
        services.AddMyEvents(config);
        services.AddMyIdentity(config);
        services.AddMyInfrastructure();
        services.AddMyIntegrations();
        services.AddMyJson(ConfigureJson);
        services.AddMyLog();
        services.AddMyMedia();
        services.AddMessagingTransport(config);
        services.AddMyMessagingChannel();
        services.AddMyMobilePushChannel();
        services.AddMyNodaTime();
        services.AddMyOpenApi();
        services.AddMySmsChannel();
        services.AddMyStorage(config);
        services.AddMySubscriptions();
        services.AddMyTelemetry(config);
        services.AddMyTemplates();
        services.AddMyTopics();
        services.AddMyUserEvents(config);
        services.AddMyUserNotifications(config);
        services.AddMyUsers();
        services.AddMyUtils();
        services.AddMyWebChannel();
        services.AddMyWebhookChannel();
        services.AddMyWebPipeline(config);
        services.AddMyWebPushChannel(config);

        services.IntegrateAmazonSES(config);
        services.IntegrateFirebase();
        services.IntegrateMailchimp();
        services.IntegrateMailjet();
        services.IntegrateMessageBird(config);
        services.IntegrateSmtp();
        services.IntegrateTelegram();
        services.IntegrateThreema();
        services.IntegrateTwilio();

        services.AddInitializer();
        services.AddBackgroundProcesses();
    }

    private static void ConfigureJson(JsonSerializerOptions options)
    {
        options.Converters.Add(new JsonSoftEnumConverter<ConfirmMode>());
        options.Converters.Add(new JsonTopicIdConverter());
    }

    public void Configure(IApplicationBuilder app)
    {
        var signalROptions = app.ApplicationServices.GetRequiredService<IOptions<SignalROptions>>().Value;

        app.UseCookiePolicy();

        app.UseDefaultPathBase();
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

        app.UseCors(builder => builder
            .SetIsOriginAllowed(x => true)
            .AllowCredentials()
            .AllowAnyMethod()
            .AllowAnyHeader());

        app.UseOpenApi(settings =>
        {
            settings.Path = "/api/openapi.json";
        });

        app.UseReDoc(settings =>
        {
            settings.Path = "/api/docs";
            settings.CustomJavaScriptPath = null;
            settings.CustomInlineStyles = null;
            settings.DocumentPath = "/api/openapi.json";
        });

        app.UseWhen(c => c.Request.Path.StartsWithSegments("/account", StringComparison.OrdinalIgnoreCase), builder =>
        {
            builder.UseExceptionHandler("/account/error");
        });

        app.UseWhen(c => c.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase), builder =>
        {
            builder.UseMiddleware<RequestExceptionMiddleware>();
        });

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            if (signalROptions.Enabled)
            {
                endpoints.MapHub<NotificationHub>("/hub");
            }

            endpoints.MapControllers();
            endpoints.MapRazorPages();
        });

        app.Map("/api", builder =>
        {
            // Return a 404 for all unresolved api requests.
            builder.Use404();
        });

        app.UseFrontend();
    }
}

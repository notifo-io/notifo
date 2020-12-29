// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using IdentityServer4.Configuration;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Logging;
using Notifo.Identity;
using Notifo.Identity.MongoDb;
using Notifo.Infrastructure.Identity;
using Squidex.Hosting;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IdentityServiceExtensions
    {
        private const string AlternativeSchema = "smart";

        public static void AddMyIdentity(this IServiceCollection services, IConfiguration config)
        {
            IdentityModelEventSource.ShowPII = true;

            var identityOptions = config.GetSection("identity").Get<NotifoIdentityOptions>() ?? new NotifoIdentityOptions();

            services.Configure<NotifoIdentityOptions>(config, "identity");

            services.AddIdentity<NotifoUser, NotifoRole>()
                .AddDefaultTokenProviders();

            services.AddSingletonAs<UserCreator>()
                .AsSelf();

            services.AddSingletonAs<UserResolver>()
                .As<IUserResolver>();

            services.AddIdentityServer(options =>
                {
                })
                .AddAspNetIdentity<NotifoUser>()
                .AddClients()
                .AddIdentityResources()
                .AddApiResources();

            services.Configure<IdentityServerOptions>((c, options) =>
            {
                var urlBuilder = c.GetRequiredService<IUrlGenerator>();

                options.IssuerUri = urlBuilder.BuildUrl();

                options.Authentication.CookieAuthenticationScheme = IdentityConstants.ApplicationScheme;

                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                options.UserInteraction.ErrorUrl = "/account/error";
            });

            services.Configure<ApiAuthorizationOptions>(options =>
            {
                options.Clients.AddIdentityServerSPA("notifo", client => client
                    .WithLogoutRedirectUri("/authentication/logout-callback")
                    .WithRedirectUri("/authentication/login-callback")
                    .WithRedirectUri("/authentication/login-silent-callback.html"));
            });

            services.AddAuthorization();
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = AlternativeSchema;
                    options.DefaultChallengeScheme = AlternativeSchema;
                })
                .AddPolicyScheme(AlternativeSchema, null, options =>
                {
                    options.ForwardDefaultSelector = context =>
                    {
                        if (ApiKeyHandler.IsApiKey(context.Request, out _))
                        {
                            return ApiKeyDefaults.AuthenticationScheme;
                        }

                        return "IdentityServerJwt";
                    };
                })
                .AddGoogle(identityOptions)
                .AddGithub(identityOptions)
                .AddApiKey()
                .AddIdentityServerJwt();
        }

        public static void AddMyMongoDbIdentity(this IServiceCollection services)
        {
            services.AddSingletonAs<MongoDbUserStore>()
                .As<IUserStore<NotifoUser>>().As<IUserFactory>();

            services.AddSingletonAs<MongoDbRoleStore>()
                .As<IRoleStore<NotifoRole>>();

            services.AddSingletonAs<MongoDbPersistedGrantStore>()
                .As<IPersistedGrantStore>();

            services.AddSingletonAs<MongoDbKeyStore>()
                .As<ISigningCredentialStore>().As<IValidationKeysStore>();

            services.AddSingletonAs<MongoDbXmlRepository>()
                .As<IXmlRepository>();

            services.Configure<KeyManagementOptions>((c, options) =>
            {
                options.XmlRepository = c.GetRequiredService<IXmlRepository>();
            });
        }
    }
}

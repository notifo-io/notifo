﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Notifo.Domain.Identity;
using Notifo.Identity;
using Notifo.Identity.ApiKey;
using Notifo.Identity.MongoDb;
using OpenIddict.Validation.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IdentityServiceExtensions
    {
        public static void AddMyIdentity(this IServiceCollection services, IConfiguration config)
        {
            IdentityModelEventSource.ShowPII = true;

            var identityOptions = config.GetSection("identity").Get<NotifoIdentityOptions>() ?? new NotifoIdentityOptions();

            services.Configure<NotifoIdentityOptions>(config, "identity");

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddDefaultTokenProviders();

            services.AddSingletonAs<UserCreator>()
                .AsSelf();

            services.AddSingletonAs<DefaultUserResolver>()
                .As<IUserResolver>();

            services.AddScopedAs<DefaultUserService>()
                .As<IUserService>();

            services.AddMyOpenIdDict();

            services.AddAuthorization();
            services.AddAuthentication()
                .AddPolicyScheme(Constants.IdentityServerOrApiKeyScheme, null, options =>
                {
                    options.ForwardDefaultSelector = context =>
                    {
                        if (ApiKeyHandler.IsApiKey(context.Request, out _))
                        {
                            return ApiKeyDefaults.AuthenticationScheme;
                        }

                        return OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
                    };
                })
                .AddGoogle(identityOptions)
                .AddGithub(identityOptions)
                .AddApiKey();
        }

        private static void AddMyOpenIdDict(this IServiceCollection services)
        {
            services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.UserIdClaimType = Claims.Subject;
                options.ClaimsIdentity.UserNameClaimType = Claims.Name;
                options.ClaimsIdentity.RoleClaimType = Claims.Role;
            });

            services.AddOpenIddict()
                .AddServer(options =>
                {
                    options
                        .SetAuthorizationEndpointUris("/connect/authorize")
                        .SetIntrospectionEndpointUris("/connect/introspect")
                        .SetLogoutEndpointUris("/connect/logout")
                        .SetUserinfoEndpointUris("/connect/userinfo");

                    options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles);

                    options.AllowImplicitFlow();

                    options.UseAspNetCore()
                        .EnableAuthorizationEndpointPassthrough()
                        .EnableLogoutEndpointPassthrough()
                        .EnableStatusCodePagesIntegration()
                        .EnableUserinfoEndpointPassthrough();
                })
                .AddValidation(options =>
                {
                    options.UseLocalServer();
                    options.UseAspNetCore();
                });

            services.AddSingletonAs<OpenIdDictWorker>()
                .As<IHostedService>();
        }

        public static void AddMyMongoDbIdentity(this IServiceCollection services)
        {
            services.AddOpenIddict()
                .AddCore(options =>
                {
                    options.UseMongoDb();
                });

            services.AddSingletonAs<MongoDbUserStore>()
                .As<IUserStore<IdentityUser>>().As<IUserFactory>();

            services.AddSingletonAs<MongoDbRoleStore>()
                .As<IRoleStore<IdentityRole>>();

            services.AddSingletonAs<MongoDbXmlRepository>()
                .As<IXmlRepository>();

            services.ConfigureOptions<MongoDbKeyOptions>();

            services.Configure<KeyManagementOptions>((c, options) =>
            {
                options.XmlRepository = c.GetRequiredService<IXmlRepository>();
            });
        }
    }
}

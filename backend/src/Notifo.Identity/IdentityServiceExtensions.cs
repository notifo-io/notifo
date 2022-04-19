// ==========================================================================
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
using Microsoft.IdentityModel.Logging;
using Notifo.Domain.Identity;
using Notifo.Identity;
using Notifo.Identity.ApiKey;
using Notifo.Identity.InMemory;
using Notifo.Identity.MongoDb;
using OpenIddict.Abstractions;
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

            services.AddSingletonAs<TokenStoreInitializer>()
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
                .AddServer(builder =>
                {
                    builder
                        .SetAuthorizationEndpointUris("/connect/authorize")
                        .SetIntrospectionEndpointUris("/connect/introspect")
                        .SetLogoutEndpointUris("/connect/logout")
                        .SetTokenEndpointUris("/connect/token")
                        .SetUserinfoEndpointUris("/connect/userinfo");

                    builder.RegisterScopes(
                        Scopes.Email,
                        Scopes.Profile,
                        Scopes.Roles,
                        Constants.ApiScope);

                    builder.AllowImplicitFlow();
                    builder.AllowAuthorizationCodeFlow();
                    builder.AllowClientCredentialsFlow();

                    builder.SetAccessTokenLifetime(TimeSpan.FromDays(30));

                    builder.UseAspNetCore()
                        .DisableTransportSecurityRequirement()
                        .EnableAuthorizationEndpointPassthrough()
                        .EnableLogoutEndpointPassthrough()
                        .EnableStatusCodePagesIntegration()
                        .EnableTokenEndpointPassthrough()
                        .EnableUserinfoEndpointPassthrough();
                })
                .AddValidation(builder =>
                {
                    builder.UseLocalServer();
                    builder.UseAspNetCore();
                });
        }

        public static void AddMyMongoDbIdentity(this IServiceCollection services)
        {
            services.AddOpenIddict()
                .AddCore(builder =>
                {
                    builder.UseMongoDb<string>();

                    builder.SetDefaultScopeEntity<ImmutableScope>();

                    builder.Services.AddSingletonAs<InMemoryConfiguration.Scopes>()
                        .As<IOpenIddictScopeStore<ImmutableScope>>();

                    builder.SetDefaultApplicationEntity<ImmutableApplication>();

                    builder.Services.AddSingletonAs<InMemoryConfiguration.Applications>()
                        .As<IOpenIddictApplicationStore<ImmutableApplication>>();

                    builder.ReplaceApplicationManager(typeof(ApplicationManager<>));
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

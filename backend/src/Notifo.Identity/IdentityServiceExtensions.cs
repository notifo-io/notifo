// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using IdentityServer4.Configuration;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Notifo.Identity;
using Notifo.Identity.MongoDb;
using Notifo.Infrastructure.Identity;

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

            services.AddIdentityServer()
                .AddAspNetIdentity<NotifoUser>()
                .AddClients()
                .AddIdentityResources()
                .AddApiResources();

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

            services.AddSingleton<IConfigureOptions<JwtBearerOptions>, IdentityOptions>();

            services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<IdentityServerOptions>, IdentityOptions>());
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

            services.AddSingleton<IConfigureOptions<KeyManagementOptions>>(s =>
            {
                return new ConfigureOptions<KeyManagementOptions>(options =>
                {
                    options.XmlRepository = s.GetRequiredService<IXmlRepository>();
                });
            });
        }

        internal class IdentityOptions : IConfigureOptions<IdentityServerOptions>, IConfigureNamedOptions<JwtBearerOptions>
        {
            private readonly UrlOptions urlOptions;

            public IdentityOptions(IOptions<UrlOptions> urlOptions)
            {
                this.urlOptions = urlOptions.Value;
            }

            public void Configure(IdentityServerOptions options)
            {
                options.Authentication.CookieAuthenticationScheme = IdentityConstants.ApplicationScheme;

                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                options.UserInteraction.ErrorUrl = "/account/error";
            }

            public void Configure(string name, JwtBearerOptions options)
            {
                var original = options.Events?.OnMessageReceived;

                options.Events ??= new JwtBearerEvents();
                options.Events.OnMessageReceived = async context =>
                {
                    if (original != null)
                    {
                        await original(context);
                    }

                    context.Options.TokenValidationParameters.ValidIssuer = urlOptions.BaseUrl.TrimEnd('/');
                };
            }

            public void Configure(JwtBearerOptions options)
            {
            }
        }
    }
}

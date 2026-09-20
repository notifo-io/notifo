// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Notifo;
using Notifo.Domain.Apps;
using Notifo.Domain.Channels.Email;
using Notifo.Domain.Channels.Messaging;
using Notifo.Domain.Channels.Sms;
using Notifo.Domain.ChannelTemplates;
using Notifo.Domain.Events;
using Notifo.Domain.Log;
using Notifo.Domain.Media;
using Notifo.Domain.Subscriptions;
using Notifo.Domain.Templates;
using Notifo.Domain.Topics;
using Notifo.Domain.UserNotifications;
using Notifo.Domain.Users;
using Notifo.Identity;
using Notifo.Identity.Dynamic;
using Notifo.Infrastructure;
using Notifo.Infrastructure.KeyValueStore;
using Notifo.Infrastructure.Migrations;
using Notifo.Infrastructure.Scheduling.Implementation;
using Notifo.Infrastructure.Scheduling.Implementation.TimerBased;
using Notifo.SqlProviders.MySql;
using Notifo.SqlProviders.MySql.App;
using Notifo.SqlProviders.Postgres;
using Notifo.SqlProviders.Postgres.App;
using Notifo.SqlProviders.SqlServer;
using Notifo.SqlProviders.SqlServer.App;
using PhenX.EntityFrameworkCore.BulkInsert.MySql;
using PhenX.EntityFrameworkCore.BulkInsert.PostgreSql;
using PhenX.EntityFrameworkCore.BulkInsert.SqlServer;
using Squidex.Messaging;

namespace Microsoft.Extensions.DependencyInjection;

public static class EFServiceExtensions
{
    public static void AddMyEntityFrameworkStore(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetRequiredValue("storage:sql:connectionString");

        void AddProvider<TContext, TParser>(Action<DbContextOptionsBuilder> configure)
            where TContext : AppDbContext
            where TParser : ConnectionStringParser
        {
            services.AddPooledDbContextFactory<TContext>(builder =>
            {
                builder.SetDefaults();
                configure(builder);
            });

            services.AddMyEntityFrameworkStore<TContext>(config);

            services.AddSingletonAs<TParser>()
                .As<ConnectionStringParser>();
        }

        config.ConfigureByOption("storage:sql:provider", new Alternatives
        {
            ["MySql"] = () =>
            {
                var versionString = config.GetValue<string>("storage:sql:version");

                var version =
                    !string.IsNullOrWhiteSpace(versionString) ?
                    ServerVersion.Parse(versionString) :
                    ServerVersion.AutoDetect(connectionString);

                AddProvider<MySqlAppDbContext, MySqlConnectionStringParser>(builder =>
                {
                    builder.UseBulkInsertMySql();
                    builder.UseMySql(connectionString, version);
                });
            },
            ["Postgres"] = () =>
            {
                AddProvider<PostgresAppDbContext, PostgresConnectionStringParser>(builder =>
                {
                    builder.UseBulkInsertPostgreSql();
                    builder.UseNpgsql(connectionString);
                });
            },
            ["SqlServer"] = () =>
            {
                AddProvider<SqlServerAppDbContext, SqlServerConnectionStringParser>(builder =>
                {
                    builder.UseBulkInsertSqlServer();
                    builder.UseSqlServer(connectionString);
                });
            },
        });
    }

    private static void AddMyEntityFrameworkStore<TContext>(this IServiceCollection services, IConfiguration config)
        where TContext : AppDbContext
    {
        if (config.GetValue("storage:sql:runMigration", true))
        {
            services.AddSingletonAs<DatabaseMigrator<TContext>>();
        }

        services.AddOpenIddict()
            .AddCore(builder =>
            {
                builder.UseEntityFrameworkCore()
                    .UseDbContext<TContext>();
            });

        services.AddScoped<IUserStore<IdentityUser>, EFUserStore<TContext>>();
        services.AddScoped<IRoleStore<IdentityRole>, RoleStore<IdentityRole, TContext>>();

        services.AddSingletonAs<EFAppRepository<TContext>>()
            .As<IAppRepository>();

        services.AddSingletonAs<EFChannelTemplateRepository<TContext, EmailTemplate>>()
            .As<IChannelTemplateRepository<EmailTemplate>>();

        services.AddSingletonAs<EFChannelTemplateRepository<TContext, MessagingTemplate>>()
            .As<IChannelTemplateRepository<MessagingTemplate>>();

        services.AddSingletonAs<EFChannelTemplateRepository<TContext, SmsTemplate>>()
            .As<IChannelTemplateRepository<SmsTemplate>>();

        services.AddSingletonAs<EFConfigurationStore<TContext, AppAuthScheme>>()
            .As<IConfigurationStore<AppAuthScheme>>();

        services.AddSingletonAs<EFEventRepository<TContext>>()
            .As<IEventRepository>();

        services.AddSingletonAs<EFKeyValueStore<TContext>>()
            .As<IKeyValueStore>();

        services.AddSingletonAs<EFLogRepository<TContext>>()
            .As<ILogRepository>();

        services.AddSingletonAs<EFMediaRepository<TContext>>()
            .As<IMediaRepository>();

        services.AddSingletonAs<EFSchedulingProvider<TContext>>()
            .As<ISchedulingProvider>();

        services.AddSingletonAs<EFSubscriptionRepository<TContext>>()
            .As<ISubscriptionRepository>();

        services.AddSingletonAs<EFTemplateRepository<TContext>>()
            .As<ITemplateRepository>();

        services.AddSingletonAs<EFTopicRepository<TContext>>()
            .As<ITopicRepository>();

        services.AddSingletonAs<EFUserFactory>()
            .As<IUserFactory>();

        services.AddSingletonAs<EFUserNotificationRepository<TContext>>()
            .As<IUserNotificationRepository>();

        services.AddSingletonAs<EFUserRepository<TContext>>()
            .As<IUserRepository>();

        services.AddSingletonAs<EFXmlRepository<TContext>>()
            .As<IXmlRepository>();

        services.ConfigureOptions<EFKeyOptions<TContext>>();
    }

    public static MessagingBuilder AddMyEntityFrameworkTransport(this MessagingBuilder messaging, IConfiguration config)
    {
        config.ConfigureByOption("storage:sql:provider", new Alternatives
        {
            ["MySql"] = () =>
            {
                messaging.AddEntityFrameworkTransport<MySqlAppDbContext>(config);
            },
            ["Postgres"] = () =>
            {
                messaging.AddEntityFrameworkTransport<PostgresAppDbContext>(config);
            },
            ["SqlServer"] = () =>
            {
                messaging.AddEntityFrameworkTransport<SqlServerAppDbContext>(config);
            },
        });

        return messaging;
    }
}

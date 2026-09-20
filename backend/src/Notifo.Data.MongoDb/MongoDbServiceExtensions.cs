// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;
using MongoDB.Driver.GridFS;
using Notifo.Domain;
using Notifo.Domain.Apps;
using Notifo.Domain.Channels.Email;
using Notifo.Domain.Channels.Messaging;
using Notifo.Domain.Channels.Sms;
using Notifo.Domain.ChannelTemplates;
using Notifo.Domain.Events;
using Notifo.Domain.Identity;
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
using Notifo.Infrastructure.Scheduling.Implementation;
using Notifo.Infrastructure.Scheduling.Implementation.TimerBased;

namespace Microsoft.Extensions.DependencyInjection;

public static class MongoDbServiceExtensions
{
    public static void AddMyMongoDbStore(this IServiceCollection services, IConfiguration config)
    {
        services.ConfigureAndValidate<MongoDbOptions>(config, "storage:mongoDb");

        NotificationSendSerializer.Register();
        SoftEnumSerializer<ConfirmMode>.Register();

        services.AddSingletonAs(c =>
            {
                var connectionString = c.GetRequiredService<IOptions<MongoDbOptions>>().Value.ConnectionString;

                return MongoClientFactory.Create(connectionString, settings =>
                {
                    settings.ClusterConfigurator = builder =>
                    {
                        builder.Subscribe(new DiagnosticsActivityEventSubscriber());
                    };
                });
            })
            .As<IMongoClient>();

        services.AddSingletonAs(c =>
            {
                var databaseName = c.GetRequiredService<IOptions<MongoDbOptions>>().Value.DatabaseName;

                return c.GetRequiredService<IMongoClient>().GetDatabase(databaseName);
            })
            .As<IMongoDatabase>();

        services.AddOpenIddict()
            .AddCore(builder =>
            {
                builder.UseMongoDb();
            });

        services.AddSingletonAs<MongoDbAppRepository>()
            .As<IAppRepository>();

        services.AddSingletonAs<MongoDbChannelTemplateRepository<EmailTemplate>>()
            .As<IChannelTemplateRepository<EmailTemplate>>();

        services.AddSingletonAs<MongoDbChannelTemplateRepository<MessagingTemplate>>()
            .As<IChannelTemplateRepository<MessagingTemplate>>();

        services.AddSingletonAs<MongoDbChannelTemplateRepository<SmsTemplate>>()
            .As<IChannelTemplateRepository<SmsTemplate>>();

        services.AddSingletonAs<MongoDbConfigurationStore<AppAuthScheme>>()
            .As<IConfigurationStore<AppAuthScheme>>();

        services.AddSingletonAs<MongoDbEventRepository>()
            .As<IEventRepository>();

        services.AddSingletonAs<MongoDbKeyValueStore>()
            .As<IKeyValueStore>();

        services.AddSingletonAs<MongoDbLogRepository>()
            .As<ILogRepository>();

        services.AddSingletonAs<MongoDbMediaRepository>()
            .As<IMediaRepository>();

        services.AddSingletonAs<MongoDbRoleStore>()
            .As<IRoleStore<IdentityRole>>();

        services.AddSingletonAs<MongoDbSchedulingProvider>()
            .As<ISchedulingProvider>();

        services.AddSingletonAs<MongoDbSubscriptionRepository>()
            .As<ISubscriptionRepository>();

        services.AddSingletonAs<MongoDbTemplateRepository>()
            .As<ITemplateRepository>();

        services.AddSingletonAs<MongoDbTokenStoreInitializer>()
            .AsSelf();

        services.AddSingletonAs<MongoDbTopicRepository>()
            .As<ITopicRepository>();

        services.AddSingletonAs<MongoDbUserNotificationRepository>()
            .As<IUserNotificationRepository>();

        services.AddSingletonAs<MongoDbUserRepository>()
            .As<IUserRepository>();

        services.AddSingletonAs<MongoDbUserStore>()
            .As<IUserStore<IdentityUser>>().As<IUserFactory>();

        services.AddSingletonAs<MongoDbXmlRepository>()
            .As<IXmlRepository>();

        services.ConfigureOptions<MongoDbKeyOptions>();
    }

    public static void AddMyMongoDbAssetStore(this IServiceCollection services, IConfiguration config)
    {
        var mongoGridFsBucketName = config.GetRequiredValue("assetStore:mongoDb:bucket");

        services.AddMongoAssetStore(c =>
        {
            var mongoDatabase = c.GetRequiredService<IMongoDatabase>();

            return new GridFSBucket<string>(mongoDatabase, new GridFSBucketOptions
            {
                BucketName = mongoGridFsBucketName
            });
        });
    }
}

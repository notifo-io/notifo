// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Notifo.Domain.UserEvents;
using Notifo.Domain.UserNotifications;
using Notifo.Domain.UserNotifications.MongoDb;
using Notifo.Infrastructure.Scheduling;
using Squidex.Messaging;

namespace Microsoft.Extensions.DependencyInjection;

public static class UserNotificationsServiceExtensions
{
    public static MessagingBuilder AddMyUserNotifications(this MessagingBuilder builder, IConfiguration config)
    {
        var options = config.GetSection("pipeline:confirms").Get<ConfirmPipelineOptions>() ?? new ConfirmPipelineOptions();

        builder.Services.ConfigureAndValidate<UserNotificationsOptions>(config, "notifications");

        builder.AddChannel(new ChannelName(options.ChannelName), true);

        builder.Configure(messaging =>
        {
            messaging.Routing.Add(x => x is ConfirmMessage, options.ChannelName);
        });

        builder.Services.AddSingletonAs<UserNotificationStore>()
            .As<IUserNotificationStore>();

        builder.Services.AddSingletonAs<UserNotificationFactory>()
            .As<IUserNotificationFactory>();

        builder.Services.AddSingletonAs<UserNotificationService>()
            .As<IUserNotificationService>().AsSelf().As<IScheduleHandler<UserEventMessage>>().As<IMessageHandler>();

        builder.Services.AddScheduler<UserEventMessage>("UserNotifications");

        return builder;
    }

    public static void AddMyMongoUserNotifications(this IServiceCollection services)
    {
        services.AddSingletonAs<MongoDbUserNotificationRepository>()
            .As<IUserNotificationRepository>();
    }
}

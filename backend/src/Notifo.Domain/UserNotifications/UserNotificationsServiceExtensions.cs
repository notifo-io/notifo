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

namespace Microsoft.Extensions.DependencyInjection
{
    public static class UserNotificationsServiceExtensions
    {
        public static void AddMyUserNotifications(this IServiceCollection services, IConfiguration config)
        {
            var options = config.GetSection("pipeline:confirms").Get<ConfirmPipelineOptions>() ?? new ConfirmPipelineOptions();

            services.ConfigureAndValidate<UserNotificationsOptions>(config, "notifications");

            services.AddMessaging(options.ChannelName, true);

            services.Configure<MessagingOptions>(messaging =>
            {
                messaging.Routing.Add(x => x is ConfirmMessage, options.ChannelName);
            });

            services.AddSingletonAs<UserNotificationStore>()
                .As<IUserNotificationStore>();

            services.AddSingletonAs<UserNotificationFactory>()
                .As<IUserNotificationFactory>();

            services.AddSingletonAs<UserNotificationService>()
                .As<IUserNotificationService>().AsSelf().As<IScheduleHandler<UserEventMessage>>().As<IMessageHandler>();

            services.AddScheduler<UserEventMessage>("UserNotifications");
        }

        public static void AddMyMongoUserNotifications(this IServiceCollection services)
        {
            services.AddSingletonAs<MongoDbUserNotificationRepository>()
                .As<IUserNotificationRepository>();
        }
    }
}

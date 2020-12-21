// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.UserEvents;
using Notifo.Domain.UserNotifications;
using Notifo.Domain.UserNotifications.MongoDb;
using Notifo.Infrastructure.Scheduling;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class UserNotificationsServiceExtensions
    {
        public static void AddMyUserNotifications(this IServiceCollection services)
        {
            services.AddSingletonAs<MongoDbUserNotificationRepository>()
                .As<IUserNotificationRepository>();

            services.AddSingletonAs<UserNotificationStore>()
                .As<IUserNotificationStore>();

            services.AddSingletonAs<UserNotificationFactory>()
                .As<IUserNotificationFactory>();

            services.AddSingletonAs<UserNotificationService>()
                .As<IUserNotificationService>().As<IScheduleHandler<UserEventMessage>>();

            services.AddScheduler<UserEventMessage>(new SchedulerOptions { QueueName = "UserNotifications" });
        }
    }
}

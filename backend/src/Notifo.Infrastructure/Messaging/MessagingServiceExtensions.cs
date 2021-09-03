// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Notifo.Infrastructure.Messaging;
using Notifo.Infrastructure.Messaging.Implementation;
using Notifo.Infrastructure.Messaging.Implementation.GooglePubSub;
using Notifo.Infrastructure.Messaging.Implementation.Kafka;
using Notifo.Infrastructure.Messaging.Implementation.RabbitMq;
using Notifo.Infrastructure.Messaging.Implementation.Scheduling;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MessagingServiceExtensions
    {
        public static void AddMyMessaging(this IServiceCollection services, IConfiguration config)
        {
            config.ConfigureByOption("messaging:type", new Alternatives
            {
                ["GooglePubSub"] = () =>
                {
                    services.AddMyGooglePubSubMessaging(config);
                },
                ["RabbitMq"] = () =>
                {
                    services.AddMyRabbitMqMessaging(config);
                },
                ["Kafka"] = () =>
                {
                    services.AddMyKafkaMessaging(config);
                },
                ["Scheduler"] = () =>
                {
                    services.AddMySchedulingMessaging();
                }
            });
        }

        public static void AddMyRabbitMqMessaging(this IServiceCollection services, IConfiguration config)
        {
            services.ConfigureAndValidate<RabbitMqOptions>(config, "messaging:rabbitMq");

            services.AddSingletonAs<RabbitMqMessagingProvider>()
                .As<IMessagingProvider>().AsSelf();

            services.AddSingletonAs<RabbitMqProducer>()
                .AsSelf();
        }

        public static void AddMyKafkaMessaging(this IServiceCollection services, IConfiguration config)
        {
            services.ConfigureAndValidate<KafkaOptions>(config, "messaging:kafka");

            services.AddSingletonAs<KafkaMessagingProvider>()
                .As<IMessagingProvider>().AsSelf();
        }

        public static void AddMyGooglePubSubMessaging(this IServiceCollection services, IConfiguration config)
        {
            services.ConfigureAndValidate<GooglePubSubOptions>(config, "messaging:googlePubSub");

            services.AddSingletonAs<GooglePubSubMessagingProvider>()
                .As<IMessagingProvider>();
        }

        public static void AddMySchedulingMessaging(this IServiceCollection services)
        {
            services.AddSingletonAs<SchedulingMessagingProvider>()
                .As<IMessagingProvider>().AsSelf();
        }

        public static void AddMessaging<T>(this IServiceCollection services, string channelName)
        {
            services.AddSingletonAs(c => c.GetRequiredService<IMessagingProvider>().GetMessaging<T>(c, channelName));

            services.AddSingletonAs<DelegatingProducer<T>>()
                .As<IMessageProducer<T>>();

            services.AddSingletonAs<DelegatingConsumer<T>>()
                .AsSelf();
        }
    }
}

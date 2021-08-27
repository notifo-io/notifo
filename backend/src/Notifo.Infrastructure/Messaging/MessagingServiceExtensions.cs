// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Notifo.Infrastructure.Messaging;
using Notifo.Infrastructure.Messaging.GooglePubSub;
using Notifo.Infrastructure.Messaging.Kafka;
using Notifo.Infrastructure.Messaging.RabbitMq;
using Notifo.Infrastructure.Messaging.Scheduling;

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

        public static void AddMyKafkaMessaging(this IServiceCollection services, IConfiguration config)
        {
            services.ConfigureAndValidate<KafkaOptions>(config, "messaging:kafka");

            services.AddSingletonAs<KafkaProvider>()
                .As<IMessagingProvider>().AsSelf();
        }

        public static void AddMySchedulingMessaging(this IServiceCollection services)
        {
            services.AddSingletonAs<SchedulingProvider>()
                .As<IMessagingProvider>().AsSelf();
        }

        public static void AddMyRabbitMqMessaging(this IServiceCollection services, IConfiguration config)
        {
            services.ConfigureAndValidate<RabbitMqOptions>(config, "messaging:rabbitMq");

            services.AddSingletonAs<RabbitMqProvider>()
                .As<IMessagingProvider>().AsSelf();

            services.AddSingletonAs<RabbitMqPrimitiveProducer>()
                .AsSelf();
        }

        public static void AddMyGooglePubSubMessaging(this IServiceCollection services, IConfiguration config)
        {
            services.ConfigureAndValidate<GooglePubSubOptions>(config, "messaging:googlePubSub");

            services.AddSingletonAs<GooglePubSubProvider>()
                .As<IMessagingProvider>();
        }

        public sealed class MessagingRegistration<T>
        {
            private readonly IServiceCollection services;
            private readonly string channelName;

            internal MessagingRegistration(IServiceCollection services, string channelName)
            {
                this.services = services;
                this.channelName = channelName;
            }

            public MessagingRegistration<T> ConsumedBy<TConsumer>() where TConsumer : IAbstractConsumer<T>
            {
                services.AddSingletonAs(c => c.GetRequiredService<IMessagingProvider>().GetConsumer<TConsumer, T>(c, channelName))
                    .AsSelf();

                return this;
            }
        }

        public static MessagingRegistration<T> AddMessaging<T>(this IServiceCollection services, string channelName)
        {
            services.AddSingletonAs(c => c.GetRequiredService<IMessagingProvider>().GetProducer<T>(c, channelName))
                .As<IAbstractProducer<T>>();

            return new MessagingRegistration<T>(services, channelName);
        }
    }
}

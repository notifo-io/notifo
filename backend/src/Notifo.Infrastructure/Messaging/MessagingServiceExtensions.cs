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

#pragma warning disable RECS0002 // Convert anonymous method to method group

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

        public static void AddMessagingProducer<T>(this IServiceCollection services, string channelName)
        {
            services.AddSingletonAs(c => c.GetRequiredService<IMessagingProvider>().GetProducer<T>(c, channelName))
                .As<IAbstractProducer<T>>();
        }

        public static void AddMessagingConsumer<TConsumer, T>(this IServiceCollection services, string channelName) where TConsumer : class, IAbstractConsumer<T>
        {
            services.AddSingletonAs<TConsumer>()
                .As<IAbstractConsumer<T>>();

            services.AddSingletonAs(c => c.GetRequiredService<IMessagingProvider>().GetConsumer<TConsumer, T>(c, channelName))
                .AsSelf();
        }
    }
}

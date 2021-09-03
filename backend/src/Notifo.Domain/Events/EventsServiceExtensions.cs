// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Notifo.Domain.Counters;
using Notifo.Domain.Events;
using Notifo.Domain.Events.MongoDb;
using Notifo.Domain.Events.Pipeline;
using Notifo.Infrastructure.Messaging;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class EventsServiceExtensions
    {
        public static void AddMyEvents(this IServiceCollection services, IConfiguration config)
        {
            var options = config.GetSection("pipeline:events").Get<EventPipelineOptions>() ?? new EventPipelineOptions();

            services.AddMessaging<EventMessage>(options.ChannelName);

            services.AddSingletonAs<EventStore>()
                .As<IEventStore>().As<ICounterTarget>();

            services.AddSingletonAs<EventConsumer>()
                .AsSelf().As<IMessageHandler<EventMessage>>();

            services.AddSingletonAs<EventPublisher>()
                .As<IEventPublisher>();
        }

        public static void AddMyMongoEvents(this IServiceCollection services)
        {
            NotificationSendSerializer.Register();

            services.AddSingletonAs<MongoDbEventRepository>()
                .As<IEventRepository>();
        }
    }
}

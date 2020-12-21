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

namespace Microsoft.Extensions.DependencyInjection
{
    public static class EventsServiceExtensions
    {
        public static void AddMyEvents(this IServiceCollection services, IConfiguration config)
        {
            var options = config.GetValue<EventPipelineOptions>("pipeline:events") ?? new EventPipelineOptions();

            services.AddMessagingConsumer<EventConsumer, EventMessage>(options.ChannelName);
            services.AddMessagingProducer<EventMessage>(options.ChannelName);

            services.AddSingletonAs<MongoDbEventRepository>()
                .As<IEventRepository>();

            services.AddSingletonAs<EventStore>()
                .As<IEventStore>().As<ICounterTarget>();

            services.AddSingletonAs<EventPublisher>()
                .As<IEventPublisher>();
        }
    }
}

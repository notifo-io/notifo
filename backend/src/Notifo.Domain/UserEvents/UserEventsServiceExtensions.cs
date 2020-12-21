// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Notifo.Domain.UserEvents;
using Notifo.Domain.UserEvents.Pipeline;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class UserEventsServiceExtensions
    {
        public static void AddMyUserEvents(this IServiceCollection services, IConfiguration config)
        {
            var options = config.GetValue<UserEventPipelineOptions>("pipeline:userEvents") ?? new UserEventPipelineOptions();

            services.AddMessagingConsumer<UserEventConsumer, UserEventMessage>(options.ChannelName);
            services.AddMessagingProducer<UserEventMessage>(options.ChannelName);

            services.AddSingletonAs<UserEventPublisher>()
                .As<IUserEventPublisher>();
        }
    }
}

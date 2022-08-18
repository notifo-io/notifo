// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Notifo.Domain.UserEvents;
using Notifo.Domain.UserEvents.Pipeline;
using Squidex.Messaging;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class UserEventsServiceExtensions
    {
        public static void AddMyUserEvents(this IServiceCollection services, IConfiguration config)
        {
            var options = config.GetSection("pipeline:userEvents").Get<UserEventPipelineOptions>() ?? new UserEventPipelineOptions();

            services.AddMessaging(new ChannelName(options.ChannelName), true);

            services.Configure<MessagingOptions>(messaging =>
            {
                messaging.Routing.Add(x => x is UserEventMessage, options.ChannelName);
            });

            services.AddSingletonAs<UserEventConsumer>()
                .AsSelf().As<IMessageHandler>();

            services.AddSingletonAs<UserEventPublisher>()
                .As<IUserEventPublisher>();
        }
    }
}

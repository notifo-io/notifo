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

namespace Microsoft.Extensions.DependencyInjection;

public static class UserEventsServiceExtensions
{
    public static MessagingBuilder AddMyUserEvents(this MessagingBuilder builder, IConfiguration config)
    {
        var options = config.GetSection("pipeline:userEvents").Get<UserEventPipelineOptions>() ?? new UserEventPipelineOptions();

        builder.AddChannel(new ChannelName(options.ChannelName), true);

        builder.Configure(messaging =>
        {
            messaging.Routing.Add(x => x is UserEventMessage, options.ChannelName);
        });

        builder.Services.AddSingletonAs<UserEventConsumer>()
            .AsSelf().As<IMessageHandler>();

        builder.Services.AddSingletonAs<UserEventPublisher>()
            .As<IUserEventPublisher>();

        return builder;
    }
}

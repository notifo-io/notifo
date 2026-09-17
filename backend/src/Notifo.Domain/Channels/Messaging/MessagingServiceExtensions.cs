// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Channels;
using Notifo.Domain.Channels.Messaging;
using Notifo.Domain.ChannelTemplates;
using Notifo.Domain.Integrations;
using Notifo.Infrastructure.Scheduling;

namespace Microsoft.Extensions.DependencyInjection;

public static class MessagingServiceExtensions
{
    public static void AddMyMessagingChannel(this IServiceCollection services)
    {
        services.AddSingletonAs<MessagingChannel>()
            .As<ICommunicationChannel>().As<IScheduleHandler<MessagingJob>>().As<ICallback<IMessagingSender>>();

        services.AddSingletonAs<MessagingFormatter>()
            .As<IMessagingFormatter>().As<IChannelTemplateFactory<MessagingTemplate>>();

        services.AddChannelTemplates<MessagingTemplate>();

        services.AddScheduler<MessagingJob>(Providers.Messaging, new SchedulerOptions
        {
            // Domain exceptions are marked as failed without a retry, so only unexpected errors like timeouts are retried.
            ExecutionRetries = [5000, 30000, 60000]
        });
    }
}

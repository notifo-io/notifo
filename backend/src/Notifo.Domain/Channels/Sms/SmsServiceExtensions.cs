// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Channels;
using Notifo.Domain.Channels.Sms;
using Notifo.Domain.ChannelTemplates;
using Notifo.Domain.Integrations;
using Notifo.Infrastructure.Scheduling;

namespace Microsoft.Extensions.DependencyInjection;

public static class SmsServiceExtensions
{
    public static void AddMySmsChannel(this IServiceCollection services)
    {
        services.AddSingletonAs<SmsChannel>()
            .As<ICommunicationChannel>().As<IScheduleHandler<SmsJob>>().As<ICallback<ISmsSender>>();

        services.AddSingletonAs<SmsFormatter>()
            .As<ISmsFormatter>().As<IChannelTemplateFactory<SmsTemplate>>();

        services.AddChannelTemplates<SmsTemplate>();

        services.AddScheduler<SmsJob>(Providers.Sms, new SchedulerOptions
        {
            // Domain exceptions are marked as failed without a retry, so only unexpected errors like timeouts are retried.
            ExecutionRetries = [5000, 30000, 60000]
        });
    }
}

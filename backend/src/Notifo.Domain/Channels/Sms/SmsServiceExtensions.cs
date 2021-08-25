// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Notifo.Domain.Channels;
using Notifo.Domain.Channels.Sms;
using Notifo.Domain.ChannelTemplates;
using Notifo.Infrastructure.Scheduling;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SmsServiceExtensions
    {
        public static void AddMySmsChannel(this IServiceCollection services)
        {
            services.AddSingletonAs<SmsChannel>()
                .As<ICommunicationChannel>();

            services.AddSingletonAs<SmsFormatter>()
                .As<ISmsFormatter>().As<IChannelTemplateFactory<SmsTemplate>>();

            services.AddChannelTemplates<SmsTemplate>();

            services.AddScheduler<SmsJob>(Providers.Sms, new SchedulerOptions
            {
                ExecutionRetries = Array.Empty<int>()
            });
        }
    }
}

// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.Extensions.Configuration;
using Notifo.Domain.Channels;
using Notifo.Domain.Channels.Sms;
using Notifo.Infrastructure.Scheduling;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SmsServiceExtensions
    {
        public static void AddMySmsChannel(this IServiceCollection services, IConfiguration config)
        {
            config.ConfigureByOption("sms:type", new Alternatives
            {
                ["MessageBird"] = () =>
                {
                    services.AddMyMessageBird(config);

                    services.AddSingletonAs<MessageBirdSmsSender>()
                        .As<ISmsSender>();
                },
                ["None"] = () =>
                {
                    services.AddSingletonAs<NoopSmsSender>()
                        .As<ISmsSender>();
                }
            });

            services.AddSingletonAs<SmsChannel>()
                .As<ICommunicationChannel>().As<IScheduleHandler<SmsJob>>();

            services.AddScheduler<SmsJob>(new SchedulerOptions { QueueName = Providers.Sms, ExecutionRetries = Array.Empty<int>() });
        }
    }
}

// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Mjml.AspNetCore;
using Notifo.Domain.Channels;
using Notifo.Domain.Channels.Email;
using Notifo.Domain.Channels.Email.Formatting;
using Notifo.Infrastructure.Scheduling;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class EmailServiceExtensions
    {
        public static void AddMyEmailChannel(this IServiceCollection services, IConfiguration config)
        {
            services.AddHttpClient("notifo-mjml");
            services.AddMjmlServices();

            services.Configure<AmazonSESOptions>(
                config.GetSection($"{Providers.Email}:ses"));

            services.AddSingletonAs<EmailChannel>()
                .As<ICommunicationChannel>().As<IScheduleHandler<EmailJob>>();

            services.AddSingletonAs<EmailVerificationUpdater>()
                .AsSelf();

            services.AddSingletonAs<EmailFormatter>()
                .As<IEmailFormatter>();

            services.AddSingletonAs<AmazonSESEmailServer>()
                .As<IEmailServer>();

            services.AddScheduler<EmailJob>(new SchedulerOptions { QueueName = Providers.Email });
        }
    }
}

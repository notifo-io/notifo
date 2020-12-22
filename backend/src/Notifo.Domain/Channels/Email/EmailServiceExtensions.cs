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
            config.ConfigureByOption("email:type", new Alternatives
            {
                ["AmazonSES"] = () =>
                {
                    services.AddAmazonSES(config);
                },
                ["SMTP"] = () =>
                {
                    services.AddSmtp(config);
                },
                ["None"] = () =>
                {
                    services.AddSingletonAs<NoopEmailServer>()
                        .As<IEmailServer>();
                }
            });

            services.AddHttpClient("notifo-mjml");
            services.AddMjmlServices();

            services.AddSingletonAs<EmailChannel>()
                .As<ICommunicationChannel>().As<IScheduleHandler<EmailJob>>();

            services.AddSingletonAs<EmailVerificationUpdater>()
                .AsSelf();

            services.AddSingletonAs<EmailFormatter>()
                .As<IEmailFormatter>();

            services.AddScheduler<EmailJob>(new SchedulerOptions { QueueName = Providers.Email });
        }

        private static void AddSmtp(this IServiceCollection services, IConfiguration config)
        {
            services.ConfigureAndValidate<SmtpOptions>(config, "email:smtp");

            services.AddSingletonAs<SmtpEmailServer>()
                .As<IEmailServer>();
        }

        private static void AddAmazonSES(this IServiceCollection services, IConfiguration config)
        {
            services.ConfigureAndValidate<AmazonSESOptions>(config, "email:amazonSES");

            services.AddSingletonAs<AmazonSESEmailServer>()
                .As<IEmailServer>();
        }
    }
}

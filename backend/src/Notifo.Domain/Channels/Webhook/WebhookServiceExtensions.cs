// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Channels;
using Notifo.Domain.Channels.Webhook;
using Notifo.Infrastructure.Scheduling;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class WebhookServiceExtensions
    {
        public static void AddMyWebhookChannel(this IServiceCollection services)
        {
            services.AddSingletonAs<WebhookChannel>()
                .As<ICommunicationChannel>().As<IScheduleHandler<WebhookJob>>();

            services.AddScheduler<WebhookJob>(new SchedulerOptions { QueueName = Providers.Webhook });
        }
    }
}

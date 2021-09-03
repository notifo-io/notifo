// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.Extensions.DependencyInjection;
using Notifo.Infrastructure.Scheduling;
using Notifo.Infrastructure.Scheduling.Implementation;

namespace Notifo.Infrastructure.Messaging.Implementation.Scheduling
{
    public sealed class SchedulingMessagingProvider : IMessagingProvider
    {
        public IMessaging<T> GetMessaging<T>(IServiceProvider serviceProvider, string channelName)
        {
            var schedulingProvider = serviceProvider.GetRequiredService<ISchedulingProvider>();

            var options = new SchedulerOptions
            {
                QueueName = $"Messaging_{channelName}",
                ExecutionRetries = Array.Empty<int>(),
                ExecuteInline = false
            };

            var scheduling = schedulingProvider.GetScheduling<Envelope<T>>(serviceProvider, options);

            return ActivatorUtilities.CreateInstance<SchedulingMessaging<T>>(serviceProvider, scheduling);
        }
    }
}

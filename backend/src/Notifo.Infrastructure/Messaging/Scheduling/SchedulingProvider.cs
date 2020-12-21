// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Notifo.Infrastructure.Initialization;
using Notifo.Infrastructure.Scheduling;

namespace Notifo.Infrastructure.Messaging.Scheduling
{
    public sealed class SchedulingProvider : IMessagingProvider
    {
        private readonly Dictionary<Type, object> instances = new Dictionary<Type, object>();

        public IInitializable GetConsumer<TConsumer, T>(IServiceProvider serviceProvider, string channelName) where TConsumer : IAbstractConsumer<T>
        {
            var scheduler = GetScheduler<T>(serviceProvider, channelName);

            return ActivatorUtilities.CreateInstance<SchedulingConsumer<TConsumer, T>>(serviceProvider, scheduler);
        }

        public IAbstractProducer<T> GetProducer<T>(IServiceProvider serviceProvider, string channelName)
        {
            var scheduler = GetScheduler<T>(serviceProvider, channelName);

            return ActivatorUtilities.CreateInstance<SchedulingProducer<T>>(serviceProvider, scheduler);
        }

        private IScheduler<T> GetScheduler<T>(IServiceProvider serviceProvider, string channelName)
        {
            if (instances.TryGetValue(typeof(T), out var value))
            {
                return (IScheduler<T>)value;
            }

            var schedulerProvider = serviceProvider.GetRequiredService<ISchedulerProvider>();

            var options = new SchedulerOptions
            {
                QueueName = $"Messaging_{channelName}",
                ExecutionRetries = Array.Empty<int>(),
                ExecuteInline = false
            };

            var scheduler = schedulerProvider.GetScheduler<T>(serviceProvider, options);

            instances[typeof(T)] = scheduler;

            return scheduler;
        }
    }
}

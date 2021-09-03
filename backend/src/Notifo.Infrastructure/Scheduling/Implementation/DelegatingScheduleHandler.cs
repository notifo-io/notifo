// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Hosting;

namespace Notifo.Infrastructure.Scheduling.Implementation
{
    public sealed class DelegatingScheduleHandler<T> : IInitializable
    {
        private readonly IScheduling<T> scheduling;
        private readonly IEnumerable<IScheduleHandler<T>> handlers;

        public string Name => $"SchedulingHandler({typeof(T).Name})";

        public int Order => int.MaxValue - 1;

        public DelegatingScheduleHandler(IScheduling<T> scheduling, IEnumerable<IScheduleHandler<T>> handlers)
        {
            this.scheduling = scheduling;
            this.handlers = handlers;
        }

        public async Task InitializeAsync(CancellationToken ct)
        {
            if (scheduling is IInitializable initializable)
            {
                await initializable.InitializeAsync(ct);
            }

            if (handlers.Any())
            {
                await scheduling.SubscribeAsync(OnSuccessAsync, OnErrorAsync, ct);
            }
        }

        public async Task ReleaseAsync(CancellationToken ct)
        {
            if (scheduling is IInitializable initializable)
            {
                await initializable.ReleaseAsync(ct);
            }
        }

        private async Task<bool> OnSuccessAsync(List<T> jobs, bool isLastAttempt, CancellationToken ct)
        {
            var result = false;

            foreach (var consumer in handlers)
            {
                result |= await consumer.HandleAsync(jobs, isLastAttempt, ct);
            }

            return result;
        }

        private async Task OnErrorAsync(List<T> jobs, Exception exception, CancellationToken ct)
        {
            foreach (var consumer in handlers)
            {
                await consumer.HandleExceptionAsync(jobs, exception);
            }
        }
    }
}

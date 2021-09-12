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
        private readonly IEnumerable<IScheduleHandler<T>> scheduleHandlers;

        public string Name => $"SchedulingHandler({typeof(T).Name})";

        public int Order => int.MaxValue - 1;

        public DelegatingScheduleHandler(IScheduling<T> scheduling, IEnumerable<IScheduleHandler<T>> scheduleHandlers)
        {
            this.scheduling = scheduling;
            this.scheduleHandlers = scheduleHandlers;
        }

        public async Task InitializeAsync(
            CancellationToken ct)
        {
            if (scheduleHandlers.Any())
            {
                await scheduling.SubscribeAsync(OnSuccessAsync, OnErrorAsync, ct);
            }
        }

        private async Task<bool> OnSuccessAsync(List<T> jobs, bool isLastAttempt,
            CancellationToken ct)
        {
            var result = false;

            foreach (var handler in scheduleHandlers)
            {
                result |= await handler.HandleAsync(jobs, isLastAttempt, ct);
            }

            return result;
        }

        private async Task OnErrorAsync(List<T> jobs, Exception exception,
            CancellationToken ct)
        {
            foreach (var handler in scheduleHandlers)
            {
                await handler.HandleExceptionAsync(jobs, exception);
            }
        }
    }
}

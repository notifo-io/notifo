// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Hosting;

namespace Notifo.Infrastructure.Scheduling.Implementation
{
    public delegate Task<bool> ScheduleSuccessCallback<T>(List<T> jobs, bool isLastAttempt,
            CancellationToken ct);

    public delegate Task ScheduleErrorCallback<T>(List<T> jobs, Exception exception,
            CancellationToken ct);

    public interface IScheduling<T> : IScheduler<T>, IInitializable
    {
        Task SubscribeAsync(ScheduleSuccessCallback<T> success, ScheduleErrorCallback<T> error,
            CancellationToken ct = default);
    }
}

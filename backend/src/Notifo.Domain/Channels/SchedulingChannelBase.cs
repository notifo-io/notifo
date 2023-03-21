// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Notifo.Domain.Integrations;
using Notifo.Domain.Log;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Scheduling;

namespace Notifo.Domain.Channels;

public abstract class SchedulingChannelBase<TJob, T> : ChannelBase<T>, IScheduleHandler<TJob> where TJob : ChannelJob
{
    protected IScheduler<TJob> Scheduler { get; }

    protected SchedulingChannelBase(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        Scheduler = serviceProvider.GetRequiredService<IScheduler<TJob>>();
    }

    protected async Task SkipAsync(List<TJob> jobs, LogMessage message)
    {
        await LogStore.LogAsync(jobs[0].Notification.AppId, message);

        await UpdateAsync(jobs, DeliveryResult.Skipped(message.Reason));
    }

    protected async Task SkipAsync(TJob job, LogMessage message)
    {
        await LogStore.LogAsync(job.Notification.AppId, message);

        await UpdateAsync(job, DeliveryResult.Skipped(message.Reason));
    }

    protected async Task UpdateAsync(List<TJob> jobs, DeliveryResult result)
    {
        foreach (var job in jobs)
        {
            await UpdateAsync(job, result);
        }
    }

    protected Task UpdateAsync(TJob job, DeliveryResult result)
    {
        return UserNotificationStore.TrackAsync(job.AsTrackingKey(Name), result);
    }

    public virtual Task HandleExceptionAsync(List<TJob> jobs, Exception exception)
    {
        return UpdateAsync(jobs, DeliveryResult.Failed());
    }

    public async Task<bool> HandleAsync(List<TJob> jobs, bool isLastAttempt,
        CancellationToken ct)
    {
        var activityLinks = jobs.SelectMany(x => x.Notification.Links());
        var activityContext = Activity.Current?.Context ?? default;

        using (Telemetry.Activities.StartActivity($"{Name}/Handle", ActivityKind.Internal, activityContext, links: activityLinks))
        {
            List<TJob>? unhandledJobs = null;

            foreach (var job in jobs)
            {
                if (await UserNotificationStore.IsHandledAsync(job, this, ct))
                {
                    await UpdateAsync(job, DeliveryResult.Skipped());
                }
                else
                {
                    unhandledJobs ??= new List<TJob>();
                    unhandledJobs.Add(job);
                }
            }

            if (unhandledJobs != null)
            {
                await SendJobsAsync(unhandledJobs, ct);
            }

            return true;
        }
    }

    protected abstract Task SendJobsAsync(List<TJob> jobs,
        CancellationToken ct);
}

// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using DiffEngine;
using Notifo.SDK;

namespace TestSuite.ApiTests;

public static class Extensions
{
    public static async Task<LogEntryDto[]> PollAsync(this ILogsClient logsClient, string appId,
        Func<LogEntryDto, bool> condition, int expectedCount = 1, TimeSpan timeout = default)
    {
        var result = Array.Empty<LogEntryDto>();

        if (timeout == default)
        {
            timeout = TimeSpan.FromSeconds(30);
        }

        using (var cts = new CancellationTokenSource(timeout))
        {
            while (!cts.IsCancellationRequested)
            {
                var response = await logsClient.GetLogsAsync(appId, cancellationToken: cts.Token);

                if (response.Items.Count >= expectedCount && (condition == null || response.Items.Any(condition)))
                {
                    result = response.Items.ToArray();
                    break;
                }

                await Task.Delay(50);
            }
        }

        return result;
    }
    public static async Task<LogEntryDto[]> PollAsync(this ILogsClient logsClient, string appId, string userId,
        Func<LogEntryDto, bool> condition, int expectedCount = 1, TimeSpan timeout = default)
    {
        var result = Array.Empty<LogEntryDto>();

        if (timeout == default)
        {
            timeout = TimeSpan.FromSeconds(30);
        }

        using (var cts = new CancellationTokenSource(timeout))
        {
            while (!cts.IsCancellationRequested)
            {
                var response = await logsClient.GetLogsAsync(appId, userId: userId, cancellationToken: cts.Token);

                if (response.Items.Count >= expectedCount && (condition == null || response.Items.Any(condition)))
                {
                    result = response.Items.ToArray();
                    break;
                }

                await Task.Delay(50);
            }
        }

        return result;
    }

    public static async Task<UserNotificationDetailsDto[]> PollAsync(this INotificationsClient notificationsClient, string appId, string userId,
        Func<UserNotificationDetailsDto, bool> condition, int expectedCount = 1, TimeSpan timeout = default)
    {
        var result = Array.Empty<UserNotificationDetailsDto>();

        if (timeout == default)
        {
            timeout = TimeSpan.FromSeconds(30);
        }

        using (var cts = new CancellationTokenSource(timeout))
        {
            while (!cts.IsCancellationRequested)
            {
                var response = await notificationsClient.GetNotificationsAsync(appId, userId, cancellationToken: cts.Token);

                if (response.Items.Count >= expectedCount && (condition == null || response.Items.Any(condition)))
                {
                    result = response.Items.ToArray();
                    break;
                }

                await Task.Delay(50);
            }
        }

        return result;
    }

    public static async Task<UserNotificationDetailsDto[]> PollCorrelatedAsync(this INotificationsClient notificationsClient, string appId, string correlationId,
        Func<UserNotificationDetailsDto, bool> condition, int expectedCount = 1, TimeSpan timeout = default)
    {
        var result = Array.Empty<UserNotificationDetailsDto>();

        if (timeout == default)
        {
            timeout = TimeSpan.FromSeconds(30);
        }

        using (var cts = new CancellationTokenSource(timeout))
        {
            while (!cts.IsCancellationRequested)
            {
                var response = await notificationsClient.GetAllNotificationsAsync(appId, correlationId: correlationId, cancellationToken: cts.Token);

                if (response.Items.Count >= expectedCount && (condition == null || response.Items.Any(condition)))
                {
                    result = response.Items.ToArray();
                    break;
                }

                await Task.Delay(50);
            }
        }

        return result;
    }

    public static async Task<UserNotificationDto[]> PollMyAsync(this INotificationsClient notificationsClient,
        Func<UserNotificationDto, bool> condition, int expectedCount = 1, TimeSpan timeout = default)
    {
        var result = Array.Empty<UserNotificationDto>();

        if (timeout == default)
        {
            timeout = TimeSpan.FromSeconds(30);
        }

        using (var cts = new CancellationTokenSource(timeout))
        {
            while (!cts.IsCancellationRequested)
            {
                var response = await notificationsClient.GetMyNotificationsAsync(cancellationToken: cts.Token);

                if (response.Items.Count >= expectedCount && (condition == null || response.Items.Any(condition)))
                {
                    result = response.Items.ToArray();
                    break;
                }

                await Task.Delay(50);
            }
        }

        return result;
    }
}

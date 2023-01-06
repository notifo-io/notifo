// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.SDK;

#pragma warning disable MA0048 // File name must match type name

namespace TestSuite.ApiTests;

public sealed class PollingArguments<T>
{
    public int ExpectedCount { get; set; } = 1;

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    public Func<T, bool> Condition { get; set; }

    public bool IsConditionMet(IReadOnlyCollection<T> values)
    {
        if (ExpectedCount > 0 && values.Count < ExpectedCount)
        {
            return false;
        }

        return Condition == null || values.Any(Condition);
    }
}

public static class Extensions
{
    public static async Task<LogEntryDto[]> PollAsync(this ILogsClient logsClient, string appId, string? userId,
        PollingArguments<LogEntryDto>? args = null)
    {
        var result = Array.Empty<LogEntryDto>();

        args ??= new PollingArguments<LogEntryDto>();

        using (var cts = new CancellationTokenSource(args.Timeout))
        {
            while (!cts.IsCancellationRequested)
            {
                var response = await logsClient.GetLogsAsync(appId, userId: userId, cancellationToken: cts.Token);

                if (args.IsConditionMet(response.Items))
                {
                    result = response.Items.ToArray();
                    break;
                }

                await Task.Delay(50);
            }
        }

        return result;
    }

    public static async Task<UserNotificationDetailsDto[]> PollAsync(this INotificationsClient notificationsClient, string appId, string? userId,
        PollingArguments<UserNotificationDetailsDto>? args = null)
    {
        var result = Array.Empty<UserNotificationDetailsDto>();

        args ??= new PollingArguments<UserNotificationDetailsDto>();

        using (var cts = new CancellationTokenSource(args.Timeout))
        {
            while (!cts.IsCancellationRequested)
            {
                var response = await notificationsClient.GetNotificationsAsync(appId, userId, cancellationToken: cts.Token);

                if (args.IsConditionMet(response.Items))
                {
                    result = response.Items.ToArray();
                    break;
                }

                await Task.Delay(50);
            }
        }

        return result;
    }

    public static async Task<UserNotificationDetailsDto[]> PollCorrelatedAsync(this INotificationsClient notificationsClient, string appId, string? correlationId,
        PollingArguments<UserNotificationDetailsDto>? args = null)
    {
        var result = Array.Empty<UserNotificationDetailsDto>();

        args ??= new PollingArguments<UserNotificationDetailsDto>();

        using (var cts = new CancellationTokenSource(args.Timeout))
        {
            while (!cts.IsCancellationRequested)
            {
                var response = await notificationsClient.GetAllNotificationsAsync(appId, correlationId: correlationId, cancellationToken: cts.Token);

                if (args.IsConditionMet(response.Items))
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
        PollingArguments<UserNotificationDto>? args = null)
    {
        var result = Array.Empty<UserNotificationDto>();

        args ??= new PollingArguments<UserNotificationDto>();

        using (var cts = new CancellationTokenSource(args.Timeout))
        {
            while (!cts.IsCancellationRequested)
            {
                var response = await notificationsClient.GetMyNotificationsAsync(cancellationToken: cts.Token);

                if (args.IsConditionMet(response.Items))
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

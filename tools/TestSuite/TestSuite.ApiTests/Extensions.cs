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
        return await PollCoreAsync(args, async ct =>
        {
            var response = await logsClient.GetLogsAsync(appId, userId: userId, cancellationToken: ct);

            return response.Items;
        });
    }

    public static async Task<UserNotificationDetailsDto[]> PollAsync(this INotificationsClient notificationsClient, string appId, string? userId,
        PollingArguments<UserNotificationDetailsDto>? args = null)
    {
        return await PollCoreAsync(args, async ct =>
        {
            var response = await notificationsClient.GetNotificationsAsync(appId, userId, cancellationToken: ct);

            return response.Items;
        });
    }

    public static async Task<UserNotificationDetailsDto[]> PollCorrelatedAsync(this INotificationsClient notificationsClient, string appId, string? correlationId,
        PollingArguments<UserNotificationDetailsDto>? args = null)
    {
        return await PollCoreAsync(args, async ct =>
        {
            var response = await notificationsClient.GetAllNotificationsAsync(appId, correlationId: correlationId, cancellationToken: ct);

            return response.Items;
        });
    }

    public static async Task<UserNotificationDto[]> PollMyAsync(this INotificationsClient notificationsClient,
        PollingArguments<UserNotificationDto>? args = null)
    {
        return await PollCoreAsync(args, async ct =>
        {
            var response = await notificationsClient.GetMyNotificationsAsync(cancellationToken: ct);

            return response.Items;
        });
    }

    public static async Task<EventDto[]> PollAsync(this IEventsClient eventsClient, string appId,
        PollingArguments<EventDto>? args = null)
    {
        return await PollCoreAsync(args, async ct =>
        {
            var response = await eventsClient.GetEventsAsync(appId, cancellationToken: ct);

            return response.Items;
        });
    }

    private static async Task<T[]> PollCoreAsync<T>(PollingArguments<T>? args, Func<CancellationToken, Task<IReadOnlyCollection<T>>> query)
    {
        var result = Array.Empty<T>();

        args ??= new PollingArguments<T>();

        // The polling timeout is not a failure, the caller asserts over the result instead.
        try
        {
            using (var cts = new CancellationTokenSource(args.Timeout))
            {
                while (!cts.IsCancellationRequested)
                {
                    var items = await query(cts.Token);

                    if (args.IsConditionMet(items))
                    {
                        result = items.ToArray();
                        break;
                    }

                    await Task.Delay(50, cts.Token);
                }
            }
        }
        catch (OperationCanceledException)
        {
        }

        return result;
    }
}

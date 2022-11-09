// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.SDK;

namespace TestSuite.ApiTests;

public static class Extensions
{
    public static async Task<LogEntryDto[]> WaitForLogEntriesAsync(this ILogsClient logsClient, string appId,
        Func<LogEntryDto, bool> condition, TimeSpan timeout)
    {
        var result = Array.Empty<LogEntryDto>();

        using (var cts = new CancellationTokenSource(timeout))
        {
            while (!cts.IsCancellationRequested)
            {
                var response = await logsClient.GetLogsAsync(appId, cancellationToken: cts.Token);

                if (response.Items.Count > 0 && (condition == null || response.Items.Any(condition)))
                {
                    result = response.Items.ToArray();
                    break;
                }

                await Task.Delay(50);
            }
        }

        return result;
    }

    public static async Task<UserNotificationDetailsDto[]> WaitForNotificationsAsync(this INotificationsClient notificationsClient, string appId, string userId,
        Func<UserNotificationDetailsDto, bool> condition, TimeSpan timeout)
    {
        var result = Array.Empty<UserNotificationDetailsDto>();

        using (var cts = new CancellationTokenSource(timeout))
        {
            while (!cts.IsCancellationRequested)
            {
                var response = await notificationsClient.GetNotificationsAsync(appId, userId, cancellationToken: cts.Token);

                if (response.Items.Count > 0 && (condition == null || response.Items.Any(condition)))
                {
                    result = response.Items.ToArray();
                    break;
                }

                await Task.Delay(50);
            }
        }

        return result;
    }

    public static async Task<UserNotificationDto[]> WaitForMyNotificationsAsyn(this INotificationsClient notificationsClient,
        Func<UserNotificationDto, bool> condition, TimeSpan timeout)
    {
        var result = Array.Empty<UserNotificationDto>();

        using (var cts = new CancellationTokenSource(timeout))
        {
            while (!cts.IsCancellationRequested)
            {
                var response = await notificationsClient.GetMyNotificationsAsync(cancellationToken: cts.Token);

                if (response.Items.Count > 0 && (condition == null || response.Items.Any(condition)))
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

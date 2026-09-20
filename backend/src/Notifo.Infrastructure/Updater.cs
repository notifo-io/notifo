// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Infrastructure;

public static class Updater
{
    private const int MaxDelayInMs = 100;

    public static async Task<T> UpdateRetriedAsync<T>(int numRetries, Func<Task<T>> action)
    {
        for (var i = 1; i <= numRetries; i++)
        {
            try
            {
                return await action();
            }
            catch (Exception ex) when (ex is InconsistentStateException or UniqueConstraintException)
            {
                if (i == numRetries)
                {
                    throw;
                }

                // Wait for a random time, otherwise concurrent updates would conflict again.
                await Task.Delay(Random.Shared.Next(MaxDelayInMs));
            }
        }

        ThrowHelper.InvalidOperationException("Invalid state reached.");
        return default!;
    }

    public static async Task UpdateRetriedAsync(int numRetries, Func<Task> action)
    {
        for (var i = 1; i <= numRetries; i++)
        {
            try
            {
                await action();
            }
            catch (Exception ex) when (ex is InconsistentStateException or UniqueConstraintException)
            {
                if (i == numRetries)
                {
                    throw;
                }

                // Wait for a random time, otherwise concurrent updates would conflict again.
                await Task.Delay(Random.Shared.Next(MaxDelayInMs));
            }
        }
    }
}

// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure.MongoDb;

namespace Notifo.Infrastructure;

public static class Updater
{
    public static async Task<T> UpdateRetriedAsync<T>(int numRetries, Func<Task<T>> action)
    {
        for (var i = 1; i <= numRetries; i++)
        {
            try
            {
                return await action();
            }
            catch (InconsistentStateException)
            {
                if (i == numRetries)
                {
                    throw;
                }
            }
            catch (UniqueConstraintException)
            {
                if (i == numRetries)
                {
                    throw;
                }
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
            catch (InconsistentStateException)
            {
                if (i == numRetries)
                {
                    throw;
                }
            }
            catch (UniqueConstraintException)
            {
                if (i == numRetries)
                {
                    throw;
                }
            }
        }
    }
}

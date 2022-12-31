// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Integrations;

public readonly struct AsyncResult<T>
{
    private readonly Func<CancellationToken, Task<T>> action;
    private readonly T value;
    private readonly TimeSpan timeout;

    public bool HasValue
    {
        get => action == null;
    }

    public T Value
    {
        get
        {
            if (!HasValue)
            {
                throw new InvalidOperationException("Value has not been computed yet.");
            }

            return value;
        }
    }

    internal AsyncResult(T value)
    {
        this.value = value;
    }

    internal AsyncResult(TimeSpan timeout, Func<CancellationToken, Task<T>> action)
    {
        this.timeout = timeout;
        this.action = action;
    }

    public async Task<T> GetValueAsync(CancellationToken ct)
    {
        if (HasValue)
        {
            throw new InvalidOperationException("Value is precomputed.");
        }

        using var cts = new CancellationTokenSource(timeout);
        using var ctl = CancellationTokenSource.CreateLinkedTokenSource(ct, cts.Token);

        return await action(ctl.Token);
    }
}

public static class AsyncResult
{
    public static AsyncResult<T> Value<T>(T value)
    {
        return new AsyncResult<T>(value);
    }

    public static AsyncResult<T> Computed<T>(TimeSpan timeout, Func<CancellationToken, Task<T>> action)
    {
        return new AsyncResult<T>(timeout, action);
    }
}

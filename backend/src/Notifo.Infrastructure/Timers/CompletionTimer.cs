// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Infrastructure.Timers;

public sealed class CompletionTimer
{
    private const int OneCallNotExecuted = 0;
    private const int OneCallExecuted = 1;
    private const int OneCallRequested = 2;
    private readonly CancellationTokenSource stopToken = new CancellationTokenSource();
    private readonly Task runTask;
    private int oneCallState;
    private CancellationTokenSource? wakeupToken;

    public CompletionTimer(int delayInMs, Func<CancellationToken, Task> callback, int initialDelay = 0)
    {
        Guard.NotNull(callback);
        Guard.GreaterThan(delayInMs, 0);

        runTask = RunInternalAsync(delayInMs, initialDelay, callback);
    }

    public async Task StopAsync()
    {
        await stopToken.CancelAsync();
        await runTask;
    }

    public void SkipCurrentDelay()
    {
        if (!stopToken.IsCancellationRequested)
        {
            Interlocked.CompareExchange(ref oneCallState, OneCallRequested, OneCallNotExecuted);

            wakeupToken?.Cancel();
        }
    }

    private async Task RunInternalAsync(int delay, int initialDelay, Func<CancellationToken, Task> callback)
    {
        try
        {
            if (initialDelay > 0)
            {
                await WaitAsync(initialDelay).ConfigureAwait(false);
            }

            while (oneCallState == OneCallRequested || !stopToken.IsCancellationRequested)
            {
                try
                {
                    await callback(stopToken.Token).ConfigureAwait(false);
                }
                catch (Exception) when (!stopToken.IsCancellationRequested)
                {
                    // A failing callback must not stop the timer, it will be called again after the delay.
                }

                oneCallState = OneCallExecuted;

                await WaitAsync(delay).ConfigureAwait(false);
            }
        }
        catch
        {
            return;
        }
    }

    private async Task WaitAsync(int intervall)
    {
        try
        {
            wakeupToken = new CancellationTokenSource();

            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(stopToken.Token, wakeupToken.Token))
            {
                await Task.Delay(intervall, cts.Token).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }
}

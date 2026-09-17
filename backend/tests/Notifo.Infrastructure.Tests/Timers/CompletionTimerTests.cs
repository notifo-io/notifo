// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Infrastructure.Timers;

public class CompletionTimerTests
{
    [Fact]
    public async Task Should_continue_if_callback_failed()
    {
        var calls = 0;
        var called = new TaskCompletionSource();

        var sut = new CompletionTimer(10, ct =>
        {
            if (Interlocked.Increment(ref calls) == 1)
            {
                throw new InvalidOperationException();
            }

            called.TrySetResult();
            return Task.CompletedTask;
        });

        await called.Task.WaitAsync(TimeSpan.FromSeconds(10));
        await sut.StopAsync();

        Assert.True(calls >= 2);
    }
}

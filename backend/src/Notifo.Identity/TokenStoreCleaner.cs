// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Notifo.Infrastructure.Timers;
using OpenIddict.Abstractions;
using Squidex.Hosting;

namespace Notifo.Identity;

public sealed class TokenStoreCleaner(IServiceProvider serviceProvider) : IInitializable
{
    private CompletionTimer timer;

    public Task InitializeAsync(
        CancellationToken ct)
    {
        timer = new CompletionTimer((int)TimeSpan.FromHours(6).TotalMilliseconds, PruneAsync);
        return Task.CompletedTask;
    }

    public Task ReleaseAsync(
        CancellationToken ct)
    {
        return timer?.StopAsync() ?? Task.CompletedTask;
    }

    private async Task PruneAsync(
        CancellationToken ct)
    {
        await using (var scope = serviceProvider.CreateAsyncScope())
        {
            var tokenManager = scope.ServiceProvider.GetRequiredService<IOpenIddictTokenManager>();

            await tokenManager.PruneAsync(DateTimeOffset.UtcNow.AddDays(-40), ct);
        }
    }
}

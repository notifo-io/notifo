// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Notifo.Identity.Dynamic;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Timers;
using Squidex.Hosting;

namespace Notifo.Identity;

public sealed class EFConfigurationStore<TContext, T>(IDbContextFactory<TContext> dbContextFactory, JsonSerializerOptions jsonOptions, IClock clock)
    : IConfigurationStore<T>, IInitializable where TContext : DbContext where T : class
{
    private CompletionTimer? timer;

    public Task InitializeAsync(
        CancellationToken ct)
    {
        timer = new CompletionTimer((int)TimeSpan.FromMinutes(10).TotalMilliseconds, CleanupAsync);
        return Task.CompletedTask;
    }

    public Task ReleaseAsync(
        CancellationToken ct)
    {
        return timer?.StopAsync() ?? Task.CompletedTask;
    }

    public async Task CleanupAsync(
        CancellationToken ct)
    {
        var now = clock.GetCurrentInstant().ToDateTimeOffset();

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

        await dbContext.Set<EFConfigurationEntity>().Where(x => x.Expires < now)
            .ExecuteDeleteAsync(ct);
    }

    public async Task<T?> GetAsync(string key,
        CancellationToken ct = default)
    {
        var now = clock.GetCurrentInstant().ToDateTimeOffset();

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

        var value =
            await dbContext.Set<EFConfigurationEntity>()
                .Where(x => x.Id == key && x.Expires > now).Select(x => x.Value)
                .FirstOrDefaultAsync(ct);

        return value != null ? JsonSerializer.Deserialize<T>(value, jsonOptions) : null;
    }

    public async Task SetAsync(string key, T value, TimeSpan ttl,
        CancellationToken ct = default)
    {
        var expires = clock.GetCurrentInstant().ToDateTimeOffset() + ttl;

        var json = JsonSerializer.Serialize(value, jsonOptions);

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

        var entity = new EFConfigurationEntity { Expires = expires, Id = key, Value = json };

        await dbContext.UpsertAsync(entity, x => x.Id == key, u => u
            .SetProperty(x => x.Value, json)
            .SetProperty(x => x.Expires, expires),
            ct);
    }
}

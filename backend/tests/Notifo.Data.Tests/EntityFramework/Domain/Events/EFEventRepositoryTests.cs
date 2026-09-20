// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NodaTime;
using Notifo.Domain;
using Notifo.Domain.Events;
using Notifo.EntityFramework.TestHelpers;
using Notifo.Infrastructure.Texts;
using Notifo.Shared;

namespace Notifo.EntityFramework.Domain.Events;

public abstract class EFEventRepositoryTests<TContext>(ISqlFixture<TContext> fixture) : EventRepositoryTests where TContext : DbContext
{
    protected override Task<IEventRepository> CreateSutAsync()
    {
        var sut = CreateSut(new EventsOptions());

        return Task.FromResult<IEventRepository>(sut);
    }

    [Fact]
    public async Task Should_cleanup_events_after_retention_time()
    {
        // Other stores use a time to live index, therefore the cleanup is only implemented for entity framework.
        var sut = CreateSut(new EventsOptions { RetentionTime = TimeSpan.FromDays(10) });

        var appId = Guid.NewGuid().ToString();
        var now = SystemClock.Instance.GetCurrentInstant();

        var oldEvent = CreateEvent(appId, now.Minus(Duration.FromDays(20)));
        var newEvent = CreateEvent(appId, now);

        await sut.InsertAsync(oldEvent);
        await sut.InsertAsync(newEvent);

        await sut.CleanupAsync(default);

        var result = await sut.QueryAsync(appId, new EventQuery());

        Assert.Equal([newEvent.Id], result.Select(x => x.Id));
    }

    private static Event CreateEvent(string appId, Instant created)
    {
        return new Event
        {
            Id = Guid.NewGuid().ToString(),
            AppId = appId,
            Created = created,
            Formatting = new NotificationFormatting<LocalizedText>
            {
                Subject = new LocalizedText
                {
                    ["en"] = "Subject"
                }
            },
            Topic = "news"
        };
    }

    private EFEventRepository<TContext> CreateSut(EventsOptions options)
    {
        return new EFEventRepository<TContext>(fixture.DbContextFactory, Options.Create(options), SystemClock.Instance);
    }
}

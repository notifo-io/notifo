// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Options;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Fixtures;
using Notifo.Infrastructure.Texts;

namespace Notifo.Domain.Events.MongoDb;

[Trait("Category", "TestContainer")]
[Collection(MongoFixtureCollection.Name)]
public class MongoDbEventRepositoryTests(MongoFixture fixture) : IAsyncLifetime
{
    private readonly MongoDbEventRepository repository = new MongoDbEventRepository(fixture.Database, Options.Create(new EventsOptions()));
    private readonly string appId = "my-app";

    public Task InitializeAsync()
    {
        return repository.InitializeAsync(default);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Should_insert_event_as_pending()
    {
        var @event = CreateEvent();

        await repository.InsertAsync(@event);

        Assert.True(await repository.IsPendingAsync(appId, @event.Id));
    }

    [Fact]
    public async Task Should_not_be_pending_after_published()
    {
        var @event = CreateEvent();

        await repository.InsertAsync(@event);
        await repository.MarkPublishedAsync(appId, @event.Id);

        Assert.False(await repository.IsPendingAsync(appId, @event.Id));
    }

    [Fact]
    public async Task Should_not_be_pending_if_event_does_not_exist()
    {
        Assert.False(await repository.IsPendingAsync(appId, Guid.NewGuid().ToString()));
    }

    [Fact]
    public async Task Should_throw_exception_if_event_already_exists()
    {
        var @event = CreateEvent();

        await repository.InsertAsync(@event);

        await Assert.ThrowsAsync<UniqueConstraintException>(() => repository.InsertAsync(@event));
    }

    [Fact]
    public async Task Should_query_published_event()
    {
        var @event = CreateEvent();

        await repository.InsertAsync(@event);
        await repository.MarkPublishedAsync(appId, @event.Id);

        var events = await repository.QueryAsync(appId, new EventQuery(), default);

        Assert.Contains(events, x => x.Id == @event.Id);
    }

    private Event CreateEvent()
    {
        return new Event
        {
            Id = Guid.NewGuid().ToString(),
            AppId = appId,
            Topic = "news",
            Formatting = new NotificationFormatting<LocalizedText>
            {
                Subject = new LocalizedText
                {
                    ["en"] = "Subject"
                }
            }
        };
    }
}

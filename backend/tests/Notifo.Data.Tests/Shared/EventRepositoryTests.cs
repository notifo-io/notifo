// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Domain;
using Notifo.Domain.Counters;
using Notifo.Domain.Events;
using Notifo.Domain.Integrations;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Texts;

namespace Notifo.Shared;

public abstract class EventRepositoryTests
{
    // Events expire after the retention time, therefore the timestamp cannot be fixed.
    private readonly Instant now = Instant.FromUnixTimeMilliseconds(SystemClock.Instance.GetCurrentInstant().ToUnixTimeMilliseconds());
    private readonly string appId = Guid.NewGuid().ToString();

    protected abstract Task<IEventRepository> CreateSutAsync();

    [Fact]
    public async Task Should_insert_event_as_pending()
    {
        var sut = await CreateSutAsync();

        var @event = CreateEvent();

        await sut.InsertAsync(@event);

        Assert.True(await sut.IsPendingAsync(appId, @event.Id));
    }

    [Fact]
    public async Task Should_not_be_pending_after_published()
    {
        var sut = await CreateSutAsync();

        var @event = CreateEvent();

        await sut.InsertAsync(@event);
        await sut.MarkPublishedAsync(appId, @event.Id);

        Assert.False(await sut.IsPendingAsync(appId, @event.Id));
    }

    [Fact]
    public async Task Should_not_be_pending_if_event_does_not_exist()
    {
        var sut = await CreateSutAsync();

        Assert.False(await sut.IsPendingAsync(appId, Guid.NewGuid().ToString()));
    }

    [Fact]
    public async Task Should_not_be_pending_for_other_app()
    {
        var sut = await CreateSutAsync();

        var @event = CreateEvent();

        await sut.InsertAsync(@event);

        Assert.False(await sut.IsPendingAsync(Guid.NewGuid().ToString(), @event.Id));
    }

    [Fact]
    public async Task Should_not_fail_if_published_event_does_not_exist()
    {
        var sut = await CreateSutAsync();

        await sut.MarkPublishedAsync(appId, Guid.NewGuid().ToString());

        var result = await sut.QueryAsync(appId, new EventQuery());

        Assert.Empty(result);
    }

    [Fact]
    public async Task Should_throw_exception_if_event_already_exists()
    {
        var sut = await CreateSutAsync();

        var @event = CreateEvent();

        await sut.InsertAsync(@event);

        await Assert.ThrowsAsync<UniqueConstraintException>(() => sut.InsertAsync(@event));
    }

    [Fact]
    public async Task Should_query_published_event()
    {
        var sut = await CreateSutAsync();

        var @event = CreateEvent();

        await sut.InsertAsync(@event);
        await sut.MarkPublishedAsync(appId, @event.Id);

        var events = await sut.QueryAsync(appId, new EventQuery(), default);

        Assert.Contains(events, x => x.Id == @event.Id);
    }

    [Fact]
    public async Task Should_insert_and_query_event()
    {
        var sut = await CreateSutAsync();

        var @event = CreateEvent();

        await sut.InsertAsync(@event);

        var result = await sut.QueryAsync(appId, new EventQuery());

        result.Single().Should().BeEquivalentTo(@event);
    }

    [Fact]
    public async Task Should_query_events_sorted_by_created()
    {
        var sut = await CreateSutAsync();

        var event1 = CreateEvent(now.Plus(Duration.FromMinutes(1)));
        var event2 = CreateEvent(now.Plus(Duration.FromMinutes(3)));
        var event3 = CreateEvent(now.Plus(Duration.FromMinutes(2)));

        await sut.InsertAsync(event1);
        await sut.InsertAsync(event2);
        await sut.InsertAsync(event3);

        var result = await sut.QueryAsync(appId, new EventQuery());

        Assert.Equal([event2.Id, event3.Id, event1.Id], result.Select(x => x.Id));
    }

    [Fact]
    public async Task Should_query_events_by_topic()
    {
        var sut = await CreateSutAsync();

        var event1 = CreateEvent();
        var event2 = CreateEvent();

        event1.Topic = "news/sport";
        event2.Topic = "news/politics";

        await sut.InsertAsync(event1);
        await sut.InsertAsync(event2);

        var result = await sut.QueryAsync(appId, new EventQuery { Query = "SPORT" });

        Assert.Equal([event1.Id], result.Select(x => x.Id));
    }

    [Fact]
    public async Task Should_query_events_by_subject_and_body()
    {
        var sut = await CreateSutAsync();

        var event1 = CreateEvent();
        var event2 = CreateEvent();
        var event3 = CreateEvent();

        event1.Formatting.Subject["en"] = "Hello World";
        event2.Formatting.Body = new LocalizedText
        {
            ["de"] = "Hallo World"
        };

        await sut.InsertAsync(event1);
        await sut.InsertAsync(event2);
        await sut.InsertAsync(event3);

        var result = await sut.QueryAsync(appId, new EventQuery { Query = "world" });

        Assert.Equal(new[] { event1.Id, event2.Id }.Order(), result.Select(x => x.Id).Order());
    }

    [Fact]
    public async Task Should_store_and_query_event_with_long_subject()
    {
        var sut = await CreateSutAsync();

        var @event = CreateEvent();

        @event.Formatting.Subject["en"] = $"Hello World {new string('x', 10000)}";

        await sut.InsertAsync(@event);

        var result = await sut.QueryAsync(appId, new EventQuery { Query = "world" });

        Assert.Equal([@event.Id], result.Select(x => x.Id));
        Assert.Equal(@event.Formatting.Subject["en"], result[0].Formatting.Subject["en"]);
    }

    [Fact]
    public async Task Should_query_events_by_channels()
    {
        var sut = await CreateSutAsync();

        var event1 = CreateEvent();
        var event2 = CreateEvent();
        var event3 = CreateEvent();

        event1.Settings[Providers.Email] = new ChannelSetting { Send = ChannelSend.Send };
        event2.Settings[Providers.Sms] = new ChannelSetting { Send = ChannelSend.Send };
        event3.Settings[Providers.Email] = new ChannelSetting { Send = ChannelSend.NotSending };

        await sut.InsertAsync(event1);
        await sut.InsertAsync(event2);
        await sut.InsertAsync(event3);

        var result = await sut.QueryAsync(appId, new EventQuery { Channels = [Providers.Email, Providers.WebPush] });

        Assert.Equal([event1.Id], result.Select(x => x.Id));
    }

    [Fact]
    public async Task Should_not_query_events_by_partial_channel_name()
    {
        var sut = await CreateSutAsync();

        var @event = CreateEvent();

        @event.Settings[Providers.WebPush] = new ChannelSetting { Send = ChannelSend.Send };

        await sut.InsertAsync(@event);

        var result = await sut.QueryAsync(appId, new EventQuery { Channels = ["push"] });

        Assert.Empty(result);
    }

    [Fact]
    public async Task Should_query_events_with_paging_and_total()
    {
        var sut = await CreateSutAsync();

        await sut.InsertAsync(CreateEvent());
        await sut.InsertAsync(CreateEvent());
        await sut.InsertAsync(CreateEvent());

        var result = await sut.QueryAsync(appId, new EventQuery { Take = 2 });

        Assert.Equal(2, result.Count);
        Assert.Equal(3, result.Total);
    }

    [Fact]
    public async Task Should_not_query_events_of_other_app()
    {
        var sut = await CreateSutAsync();

        var @event = CreateEvent();

        @event.AppId = Guid.NewGuid().ToString();

        await sut.InsertAsync(@event);

        var result = await sut.QueryAsync(appId, new EventQuery());

        Assert.Empty(result);
    }

    [Fact]
    public async Task Should_write_counters_to_existing_event()
    {
        var sut = await CreateSutAsync();

        var @event = CreateEvent();

        // The event store initializes the counters, because they cannot be incremented otherwise.
        @event.Counters = [];

        await sut.InsertAsync(@event);

        await sut.BatchWriteAsync(
        [
            ((appId, @event.Id), new CounterMap { ["counter1"] = 1 })
        ], default);

        await sut.BatchWriteAsync(
        [
            ((appId, @event.Id), new CounterMap { ["counter1"] = 2, ["counter2"] = 5 })
        ], default);

        var result = (await sut.QueryAsync(appId, new EventQuery())).Single();

        Assert.Equal(3, result.Counters!["counter1"]);
        Assert.Equal(5, result.Counters!["counter2"]);
    }

    [Fact]
    public async Task Should_not_create_event_when_writing_counters()
    {
        var sut = await CreateSutAsync();

        var eventId = Guid.NewGuid().ToString();

        await sut.BatchWriteAsync(
        [
            ((appId, eventId), new CounterMap { ["counter1"] = 1 })
        ], default);

        var result = await sut.QueryAsync(appId, new EventQuery());

        Assert.Empty(result);
    }

    [Fact]
    public async Task Should_ignore_empty_counters()
    {
        var sut = await CreateSutAsync();

        var @event = CreateEvent();

        // The event store initializes the counters, because they cannot be incremented otherwise.
        @event.Counters = [];

        await sut.InsertAsync(@event);

        await sut.BatchWriteAsync(
        [
            ((appId, @event.Id), [])
        ], default);

        var result = (await sut.QueryAsync(appId, new EventQuery())).Single();

        Assert.Empty(result.Counters!);
    }

    private Event CreateEvent(Instant created = default)
    {
        return new Event
        {
            Id = Guid.NewGuid().ToString(),
            AppId = appId,
            Created = created == default ? now : created,
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

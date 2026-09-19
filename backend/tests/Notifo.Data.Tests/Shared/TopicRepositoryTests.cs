// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Domain.Counters;
using Notifo.Domain.Topics;
using Notifo.Infrastructure.MongoDb;
using Notifo.Infrastructure.Texts;

namespace Notifo.Domain.Shared;

public abstract class TopicRepositoryTests
{
    private readonly Instant now = Instant.FromUtc(2024, 1, 2, 10, 30, 0);
    private readonly string appId = Guid.NewGuid().ToString();

    protected abstract Task<ITopicRepository> CreateSutAsync();

    [Fact]
    public async Task Should_insert_and_get_topic()
    {
        var sut = await CreateSutAsync();

        var topic = CreateTopic("news/sport");

        await sut.UpsertAsync(topic);

        var (result, etag) = await sut.GetAsync(appId, topic.Path);

        result.Should().BeEquivalentTo(topic);
        Assert.NotNull(etag);
    }

    [Fact]
    public async Task Should_return_null_if_topic_not_found()
    {
        var sut = await CreateSutAsync();

        var (result, etag) = await sut.GetAsync(appId, "unknown");

        Assert.Null(result);
        Assert.Null(etag);
    }

    [Fact]
    public async Task Should_update_topic_with_etag()
    {
        var sut = await CreateSutAsync();

        var topic = CreateTopic("news");

        await sut.UpsertAsync(topic);

        var (_, etag) = await sut.GetAsync(appId, topic.Path);

        var updated = topic with { ShowAutomatically = true };

        await sut.UpsertAsync(updated, etag);

        var (result, _) = await sut.GetAsync(appId, topic.Path);

        Assert.True(result!.ShowAutomatically);
    }

    [Fact]
    public async Task Should_throw_exception_if_etag_does_not_match()
    {
        var sut = await CreateSutAsync();

        var topic = CreateTopic("news");

        await sut.UpsertAsync(topic);

        await Assert.ThrowsAsync<InconsistentStateException>(() => sut.UpsertAsync(topic, "invalid"));
    }

    [Fact]
    public async Task Should_delete_topic()
    {
        var sut = await CreateSutAsync();

        var topic = CreateTopic("news");

        await sut.UpsertAsync(topic);
        await sut.DeleteAsync(appId, topic.Path);

        var (result, _) = await sut.GetAsync(appId, topic.Path);

        Assert.Null(result);
    }

    [Fact]
    public async Task Should_query_topics_by_scope()
    {
        var sut = await CreateSutAsync();

        await sut.UpsertAsync(CreateTopic("explicit") with { IsExplicit = true });
        await sut.UpsertAsync(CreateTopic("implicit"));

        var explicitTopics = await sut.QueryAsync(appId, new TopicQuery { Scope = TopicQueryScope.Explicit });
        var implicitTopics = await sut.QueryAsync(appId, new TopicQuery { Scope = TopicQueryScope.Implicit });
        var allTopics = await sut.QueryAsync(appId, new TopicQuery { Scope = TopicQueryScope.All });

        Assert.Equal(["explicit"], explicitTopics.Select(x => x.Path));
        Assert.Equal(["implicit"], implicitTopics.Select(x => x.Path));
        Assert.Equal(["explicit", "implicit"], allTopics.Select(x => x.Path).Order());
    }

    [Fact]
    public async Task Should_query_topics_by_text()
    {
        var sut = await CreateSutAsync();

        await sut.UpsertAsync(CreateTopic("news/sport"));
        await sut.UpsertAsync(CreateTopic("news/politics"));

        var result = await sut.QueryAsync(appId, new TopicQuery { Query = "SPORT" });

        Assert.Equal(["news/sport"], result.Select(x => x.Path));
    }

    [Fact]
    public async Task Should_query_topics_with_paging_and_total()
    {
        var sut = await CreateSutAsync();

        await sut.UpsertAsync(CreateTopic("topic1"));
        await sut.UpsertAsync(CreateTopic("topic2"));
        await sut.UpsertAsync(CreateTopic("topic3"));

        var result = await sut.QueryAsync(appId, new TopicQuery { Take = 2 });

        Assert.Equal(2, result.Count);
        Assert.Equal(3, result.Total);
    }

    [Fact]
    public async Task Should_not_query_topics_of_other_app()
    {
        var sut = await CreateSutAsync();

        await sut.UpsertAsync(CreateTopic("news") with { AppId = Guid.NewGuid().ToString() });

        var result = await sut.QueryAsync(appId, new TopicQuery());

        Assert.Empty(result);
    }

    [Fact]
    public async Task Should_write_counters_to_existing_topic()
    {
        var sut = await CreateSutAsync();

        var topic = CreateTopic("news");

        await sut.UpsertAsync(topic);

        await sut.BatchWriteAsync(
        [
            ((appId, topic.Path), new CounterMap { ["counter1"] = 1 })
        ], default);

        await sut.BatchWriteAsync(
        [
            ((appId, topic.Path), new CounterMap { ["counter1"] = 2, ["counter2"] = 5 })
        ], default);

        var (result, _) = await sut.GetAsync(appId, topic.Path);

        Assert.Equal(3, result!.Counters["counter1"]);
        Assert.Equal(5, result!.Counters["counter2"]);
    }

    [Fact]
    public async Task Should_create_topic_when_writing_counters()
    {
        var sut = await CreateSutAsync();

        await sut.BatchWriteAsync(
        [
            ((appId, "implicit/topic"), new CounterMap { ["counter1"] = 1 })
        ], default);

        var (result, _) = await sut.GetAsync(appId, "implicit/topic");

        Assert.NotNull(result);
        Assert.False(result.IsExplicit);
        Assert.Equal(1, result.Counters["counter1"]);
    }

    private Topic CreateTopic(string path)
    {
        return new Topic(appId, path, now)
        {
            LastUpdate = now,
            Name = new LocalizedText
            {
                ["en"] = path
            }
        };
    }
}

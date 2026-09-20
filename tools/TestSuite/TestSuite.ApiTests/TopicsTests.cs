// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.SDK;
using TestSuite.Fixtures;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

public class TopicsTests : IClassFixture<CreatedAppFixture>
{
    public CreatedAppFixture _ { get; set; }

    public TopicsTests(CreatedAppFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public async Task Should_create_topics()
    {
        // STEP 0: Create topics.
        var topicPath1 = $"news/{Guid.NewGuid()}";
        var topicPath2 = $"news/{Guid.NewGuid()}";

        var topicRequest = new UpsertTopicsDto
        {
            Requests =
            [
                new UpsertTopicDto
                {
                    Path = topicPath1,
                    Name = new LocalizedText
                    {
                        ["en"] = "name1_0",
                        ["de"] = "name1_0_de"
                    },
                    Description = new LocalizedText
                    {
                        ["en"] = "description1_0"
                    },
                    ShowAutomatically = true
                },
                new UpsertTopicDto
                {
                    Path = topicPath2,
                    Name = new LocalizedText
                    {
                        ["en"] = "name2_0"
                    }
                },
            ]
        };

        var topics = await _.Client.Topics.PostTopicsAsync(_.AppId, topicRequest);

        Assert.Equal(2, topics.Count);

        var topic1 = topics.ElementAt(0);
        var topic2 = topics.ElementAt(1);

        Assert.Equal(topicPath1, topic1.Path);
        Assert.Equal(topicPath2, topic2.Path);
        Assert.Equal("name1_0", topic1.Name["en"]);
        Assert.Equal("name2_0", topic2.Name["en"]);
        Assert.True(topic1.IsExplicit);
        Assert.True(topic1.ShowAutomatically);
        Assert.False(topic2.ShowAutomatically);


        // STEP 1: Query topics.
        var queried = await _.Client.Topics.GetTopicsAsync(_.AppId, TopicQueryScope.Explicit, topicPath1);

        Assert.Equal(topicPath1, queried.Items.SingleOrDefault()?.Path);

        // The path contains a guid that verify does not scrub, it is asserted explicitly instead.
        await Verify(topic1)
            .IgnoreMembersWithType<DateTimeOffset>()
            .IgnoreMember<TopicDto>(x => x.Path);
    }

    [Fact]
    public async Task Should_update_topics()
    {
        // STEP 0: Create topics.
        var topicPath1 = $"news/{Guid.NewGuid()}";
        var topicPath2 = $"news/{Guid.NewGuid()}";

        var topicRequest = new UpsertTopicsDto
        {
            Requests =
            [
                new UpsertTopicDto
                {
                    Path = topicPath1,
                    Name = new LocalizedText
                    {
                        ["en"] = "name1_0"
                    },
                    ShowAutomatically = false
                },
                new UpsertTopicDto
                {
                    Path = topicPath2,
                    Name = new LocalizedText
                    {
                        ["en"] = "name2_0"
                    }
                },
            ]
        };

        await _.Client.Topics.PostTopicsAsync(_.AppId, topicRequest);


        // STEP 1: Update topic.
        var topicRequest2 = new UpsertTopicsDto
        {
            Requests =
            [
                new UpsertTopicDto
                {
                    Path = topicPath1,
                    Name = new LocalizedText
                    {
                        ["en"] = "name1_1"
                    },
                    ShowAutomatically = true
                },
            ]
        };

        await _.Client.Topics.PostTopicsAsync(_.AppId, topicRequest2);


        // Get topics.
        var topics = await _.Client.Topics.GetTopicsAsync(_.AppId, TopicQueryScope.Explicit, take: 100000);

        var topic1 = topics.Items.SingleOrDefault(x => x.Path == topicPath1);
        var topic2 = topics.Items.SingleOrDefault(x => x.Path == topicPath2);

        Assert.Equal("name1_1", topic1?.Name["en"]);
        Assert.Equal("name2_0", topic2?.Name["en"]);
        Assert.True(topic1?.ShowAutomatically);

        // The path contains a guid that verify does not scrub, it is asserted explicitly instead.
        await Verify(topic1)
            .IgnoreMembersWithType<DateTimeOffset>()
            .IgnoreMember<TopicDto>(x => x.Path);
    }

    [Fact]
    public async Task Should_delete_topic()
    {
        // STEP 0: Create topics.
        var topicPath1 = $"news/{Guid.NewGuid()}";
        var topicPath2 = $"news/{Guid.NewGuid()}";

        var topicRequest = new UpsertTopicsDto
        {
            Requests =
            [
                new UpsertTopicDto
                {
                    Path = topicPath1,
                    Name = new LocalizedText
                    {
                        ["en"] = "name1"
                    }
                },
                new UpsertTopicDto
                {
                    Path = topicPath2,
                    Name = new LocalizedText
                    {
                        ["en"] = "name2"
                    }
                },
            ]
        };

        await _.Client.Topics.PostTopicsAsync(_.AppId, topicRequest);


        // STEP 1: Delete topic.
        await _.Client.Topics.DeleteTopicAsync(_.AppId, topicPath1);


        // Get topics.
        var topics = await _.Client.Topics.GetTopicsAsync(_.AppId, TopicQueryScope.Explicit, take: 100000);

        var topic1 = topics.Items.SingleOrDefault(x => x.Path == topicPath1);
        var topic2 = topics.Items.SingleOrDefault(x => x.Path == topicPath2);

        Assert.Null(topic1);
        Assert.NotNull(topic2);
    }
}

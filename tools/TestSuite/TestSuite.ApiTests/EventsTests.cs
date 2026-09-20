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

public class EventsTests : IClassFixture<CreatedAppFixture>
{
    public CreatedAppFixture _ { get; set; }

    public EventsTests(CreatedAppFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public async Task Should_cancel_unknown_event()
    {
        var hasCancelled = await PollCancelAsync(Guid.NewGuid().ToString(), "event");

        Assert.False(hasCancelled);
    }

    [Fact]
    public async Task Should_cancel_known_event()
    {
        // STEP 1: Create user
        var user_0 = await _.CreateUserAsync();


        // STEP 2: Publish event.
        var eventId = Guid.NewGuid().ToString();

        var publishRequest = new PublishManyDto
        {
            Requests =
            [
                new PublishDto
                {
                    Topic = $"users/{user_0.Id}",
                    Preformatted = new NotificationFormattingDto
                    {
                        Subject = new LocalizedText
                        {
                            ["en"] = Guid.NewGuid().ToString()
                        }
                    },
                    Scheduling = new SchedulingDto
                    {
                        Date = DateTimeOffset.UtcNow.AddDays(20),
                        Time = new TimeSpan(12, 0, 0),
                    },
                    Id = eventId,
                },
            ]
        };

        await _.Client.Events.PostEventsAsync(_.AppId, publishRequest);


        // STEP 3: Retry until deleted.
        var hasCancelled = await PollCancelAsync(user_0.Id, eventId);

        Assert.True(hasCancelled);
    }

    [Fact]
    public async Task Should_query_events()
    {
        var subject = Guid.NewGuid().ToString();

        // STEP 0: Create user.
        var user_0 = await _.CreateUserAsync();

        var topic = $"users/{user_0.Id}";


        // STEP 1: Publish event.
        var publishRequest = new PublishManyDto
        {
            Requests =
            [
                new PublishDto
                {
                    Topic = topic,
                    Preformatted = new NotificationFormattingDto
                    {
                        Subject = new LocalizedText
                        {
                            ["en"] = subject
                        }
                    },
                    Properties = new NotificationProperties
                    {
                        ["custom"] = "value"
                    }
                },
            ]
        };

        await _.Client.Events.PostEventsAsync(_.AppId, publishRequest);


        // Test that the event has been stored.
        var args = new PollingArguments<EventDto>
        {
            Condition = x => x.Topic == topic
        };

        var events = await _.Client.Events.PollAsync(_.AppId, args);

        var event_0 = events.SingleOrDefault(x => x.Topic == topic);

        Assert.NotNull(event_0);
        Assert.Equal(subject, event_0.Formatting.Subject["en"]);
        Assert.Equal("value", event_0.Properties["custom"]);
    }

    [Fact]
    public async Task Should_publish_own_event()
    {
        var subject = Guid.NewGuid().ToString();

        // STEP 0: Create user.
        var user_0 = await _.CreateUserAsync();

        var client = _.BuildUserClient(user_0);


        // STEP 1: Publish an event as the user itself.
        // The topic is required by the model, but the server overrides it with the topic of the user.
        var publishRequest = new PublishDto
        {
            Topic = $"users/{user_0.Id}",
            Preformatted = new NotificationFormattingDto
            {
                Subject = new LocalizedText
                {
                    ["en"] = subject
                }
            }
        };

        await client.Events.PostMyEventsAsync(publishRequest);


        // Test that the user got the notification, the topic is derived from the api key.
        var notifications = await client.Notifications.PollMyAsync();

        Assert.Contains(notifications, x => x.Subject == subject);
    }

    private async Task<bool> PollCancelAsync(string userId, string eventId)
    {
        var request = new CancelEventDto { UserId = userId, Test = false, EventId = eventId };

        // The cancellation is not a failure, the caller asserts over the result instead.
        try
        {
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
            {
                while (!cts.IsCancellationRequested)
                {
                    var result = await _.Client.Events.CancelEventAsync(_.AppId, request, cts.Token);

                    if (result.HasCancelled)
                    {
                        return true;
                    }

                    await Task.Delay(50, cts.Token);
                }
            }
        }
        catch (OperationCanceledException)
        {
        }

        return false;
    }
}

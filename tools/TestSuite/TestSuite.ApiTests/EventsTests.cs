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
        var userRequest = new UpsertUsersDto
        {
            Requests =
            [
                new UpsertUserDto(),
            ]
        };

        var users_0 = await _.Client.Users.PostUsersAsync(_.AppId, userRequest);
        var user_0 = users_0.First();


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
                        Date = DateTime.Now.AddDays(20),
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

    private async Task<bool> PollCancelAsync(string userId, string eventId)
    {
        var request = new CancelEventDto { UserId = userId, Test = false, EventId = eventId };

        using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
        {
            while (!cts.IsCancellationRequested)
            {
                var result = await _.Client.Events.CancelEventAsync(_.AppId, request);

                if (result.HasCancelled)
                {
                    return true;
                }
            }
        }

        return false;
    }
}

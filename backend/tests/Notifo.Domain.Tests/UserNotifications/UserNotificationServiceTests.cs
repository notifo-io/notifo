// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using NodaTime;
using Notifo.Domain.Apps;
using Notifo.Domain.Channels;
using Notifo.Domain.Log;
using Notifo.Domain.UserEvents;
using Notifo.Domain.Users;
using Notifo.Infrastructure.Scheduling;
using Squidex.Messaging;

namespace Notifo.Domain.UserNotifications;

public class UserNotificationServiceTests
{
    private readonly IClock clock = A.Fake<IClock>();
    private readonly IScheduler<UserEventMessage> userEventQueue = A.Fake<IScheduler<UserEventMessage>>();
    private readonly IUserStore userStore = A.Fake<IUserStore>();
    private readonly Instant now = Instant.FromUtc(2020, 1, 1, 12, 0, 0);
    private readonly UserNotificationService sut;

    public UserNotificationServiceTests()
    {
        A.CallTo(() => clock.GetCurrentInstant())
            .Returns(now);

        sut = new UserNotificationService(
            Enumerable.Empty<ICommunicationChannel>(),
            A.Fake<IAppStore>(),
            A.Fake<ILogger<UserNotificationService>>(),
            A.Fake<ILogStore>(),
            A.Fake<IMessageBus>(),
            userEventQueue,
            A.Fake<IUserNotificationFactory>(),
            A.Fake<IUserNotificationStore>(),
            userStore,
            clock);
    }

    [Fact]
    public async Task Should_prefer_event_scheduling_over_user_scheduling()
    {
        var user = new User("app", "user", default)
        {
            Scheduling = new Scheduling { DelayInSeconds = 1000 }
        };

        var userEvent = CreateUserEvent();

        userEvent.Scheduling = new Scheduling { DelayInSeconds = 10 };

        A.CallTo(() => userStore.GetCachedAsync("app", "user", A<CancellationToken>._))
            .Returns(user);

        await sut.DistributeAsync(userEvent);

        A.CallTo(() => userEventQueue.ScheduleGroupedAsync(userEvent.EventId, A<string>._, userEvent, now.Plus(Duration.FromSeconds(10)), true, A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_use_user_scheduling_if_event_has_no_scheduling()
    {
        var user = new User("app", "user", default)
        {
            Scheduling = new Scheduling { DelayInSeconds = 1000 }
        };

        var userEvent = CreateUserEvent();

        A.CallTo(() => userStore.GetCachedAsync("app", "user", A<CancellationToken>._))
            .Returns(user);

        await sut.DistributeAsync(userEvent);

        A.CallTo(() => userEventQueue.ScheduleGroupedAsync(userEvent.EventId, A<string>._, userEvent, now.Plus(Duration.FromSeconds(1000)), true, A<CancellationToken>._))
            .MustHaveHappened();
    }

    private static UserEventMessage CreateUserEvent()
    {
        return new UserEventMessage
        {
            AppId = "app",
            EventId = Guid.NewGuid().ToString(),
            Topic = "topic",
            UserId = "user"
        };
    }
}

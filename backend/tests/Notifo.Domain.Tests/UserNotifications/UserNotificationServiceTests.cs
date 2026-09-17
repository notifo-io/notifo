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
using Notifo.Domain.Integrations;
using Notifo.Domain.Log;
using Notifo.Domain.UserEvents;
using Notifo.Domain.Users;
using Notifo.Infrastructure.Scheduling;
using Squidex.Messaging;

namespace Notifo.Domain.UserNotifications;

public class UserNotificationServiceTests
{
    private readonly IAppStore appStore = A.Fake<IAppStore>();
    private readonly IClock clock = A.Fake<IClock>();
    private readonly IScheduler<UserEventMessage> userEventQueue = A.Fake<IScheduler<UserEventMessage>>();
    private readonly IUserNotificationFactory userNotificationFactory = A.Fake<IUserNotificationFactory>();
    private readonly IUserNotificationStore userNotificationsStore = A.Fake<IUserNotificationStore>();
    private readonly IUserStore userStore = A.Fake<IUserStore>();
    private readonly Instant now = Instant.FromUtc(2020, 1, 1, 12, 0, 0);
    private readonly App app = new App("app", default);
    private readonly User user = new User("app", "user", default);
    private readonly UserNotificationService sut;

    public UserNotificationServiceTests()
    {
        A.CallTo(() => clock.GetCurrentInstant())
            .Returns(now);

        A.CallTo(() => appStore.GetCachedAsync(app.Id, A<CancellationToken>._))
            .Returns(app);

        A.CallTo(() => userStore.GetCachedAsync(app.Id, user.Id, A<CancellationToken>._))
            .Returns(user);

        sut = new UserNotificationService(
            Enumerable.Empty<ICommunicationChannel>(),
            appStore,
            A.Fake<ILogger<UserNotificationService>>(),
            A.Fake<ILogStore>(),
            A.Fake<IMessageBus>(),
            userEventQueue,
            userNotificationFactory,
            userNotificationsStore,
            userStore,
            clock);
    }

    [Fact]
    public async Task Should_use_previous_event_of_group_if_last_event_is_invalid()
    {
        var userEvent1 = CreateUserEvent();
        var userEvent2 = CreateUserEvent();

        var notification = new UserNotification
        {
            Id = Guid.NewGuid(),
            AppId = app.Id,
            UserId = user.Id,
            Formatting = new NotificationFormatting<string>
            {
                Subject = "Subject"
            }
        };

        A.CallTo(() => userNotificationFactory.Create(app, user, userEvent2, A<IEnumerable<UserEventMessage>>._))
            .Returns(null);

        A.CallTo(() => userNotificationFactory.Create(app, user, userEvent1, A<IEnumerable<UserEventMessage>>._))
            .Returns(notification);

        await sut.DistributeScheduledAsync(userEvent2, [userEvent1], false);

        A.CallTo(() => userNotificationsStore.InsertAsync(notification, A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_track_failure_if_job_has_been_aborted()
    {
        var userEvent1 = CreateUserEvent();
        var userEvent2 = CreateUserEvent();

        await sut.HandleExceptionAsync([userEvent1, userEvent2], new InvalidOperationException());

        A.CallTo(() => userNotificationsStore.TrackAsync(userEvent1, A<DeliveryResult>.That.Matches(x => x.Status == DeliveryStatus.Failed), A<CancellationToken>._))
            .MustHaveHappened();

        A.CallTo(() => userNotificationsStore.TrackAsync(userEvent2, A<DeliveryResult>.That.Matches(x => x.Status == DeliveryStatus.Failed), A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_prefer_event_scheduling_over_user_scheduling()
    {
        var scheduledUser = new User("app", "user", default)
        {
            Scheduling = new Scheduling { DelayInSeconds = 1000 }
        };

        var userEvent = CreateUserEvent();

        userEvent.Scheduling = new Scheduling { DelayInSeconds = 10 };

        A.CallTo(() => userStore.GetCachedAsync("app", "user", A<CancellationToken>._))
            .Returns(scheduledUser);

        await sut.DistributeAsync(userEvent);

        A.CallTo(() => userEventQueue.ScheduleGroupedAsync(userEvent.EventId, A<string>._, userEvent, now.Plus(Duration.FromSeconds(10)), true, A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_use_user_scheduling_if_event_has_no_scheduling()
    {
        var scheduledUser = new User("app", "user", default)
        {
            Scheduling = new Scheduling { DelayInSeconds = 1000 }
        };

        var userEvent = CreateUserEvent();

        A.CallTo(() => userStore.GetCachedAsync("app", "user", A<CancellationToken>._))
            .Returns(scheduledUser);

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
            Formatting = new NotificationFormatting<Infrastructure.Texts.LocalizedText>
            {
                Subject = new Infrastructure.Texts.LocalizedText
                {
                    ["en"] = "Subject"
                }
            },
            Topic = "topic",
            UserId = "user"
        };
    }
}

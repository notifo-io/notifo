// ==========================================================================
//  Notifo.io
//  ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NodaTime;
using Notifo.Domain.Apps;
using Notifo.Domain.Integrations;
using Notifo.Domain.Log;
using Notifo.Domain.UserNotifications;
using Notifo.Domain.Users;
using Notifo.Infrastructure.Collections;
using Notifo.Infrastructure.Mediator;
using Notifo.Infrastructure.Scheduling;

namespace Notifo.Domain.Channels.MobilePush;

public class MobilePushChannelTests
{
    private readonly IScheduler<MobilePushJob> scheduler = A.Fake<IScheduler<MobilePushJob>>();
    private readonly IUserNotificationStore userNotificationStore = A.Fake<IUserNotificationStore>();
    private readonly App app = new App("app", default);
    private readonly MobilePushToken token = new MobilePushToken
    {
        Token = "token",
        DeviceType = MobileDeviceType.iOS
    };

    private readonly MobilePushChannel sut;

    public MobilePushChannelTests()
    {
        var serviceProvider =
            new ServiceCollection()
                .AddSingleton(A.Fake<IAppStore>())
                .AddSingleton(A.Fake<IIntegrationManager>())
                .AddSingleton(userNotificationStore)
                .AddSingleton(A.Fake<IUserStore>())
                .AddSingleton(A.Fake<ILogger<MobilePushChannel>>())
                .AddSingleton(A.Fake<ILogStore>())
                .AddSingleton(A.Fake<IMediator>())
                .AddSingleton(scheduler)
                .BuildServiceProvider();

        sut = new MobilePushChannel(serviceProvider);
    }

    [Fact]
    public async Task Should_schedule_wakeup_if_notification_has_been_seen()
    {
        await sut.HandleSeenAsync(CreateNotification(), CreateContext(token.Token));

        A.CallTo(() => scheduler.ScheduleAsync(A<string>._, A<MobilePushJob>._, A<Instant>._, false, A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_not_track_wakeup_job()
    {
        var wakeupNotification = new UserNotification
        {
            AppId = app.Id,
            UserId = "user"
        };

        var context = CreateContext(token.Token);

        await sut.HandleAsync([new MobilePushJob(wakeupNotification, context, token)], false, default);

        A.CallTo(userNotificationStore)
            .Where(x => x.Method.Name == nameof(IUserNotificationStore.TrackAsync))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_schedule_wakeup_if_configuration_has_no_token()
    {
        await sut.HandleSeenAsync(CreateNotification(), CreateContext(null));

        A.CallTo(() => scheduler.ScheduleAsync(A<string>._, A<MobilePushJob>._, A<Instant>._, A<bool>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_schedule_wakeup_if_token_has_been_removed()
    {
        await sut.HandleSeenAsync(CreateNotification(), CreateContext("other-token"));

        A.CallTo(() => scheduler.ScheduleAsync(A<string>._, A<MobilePushJob>._, A<Instant>._, A<bool>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    private UserNotification CreateNotification()
    {
        return new UserNotification
        {
            Id = Guid.NewGuid(),
            AppId = app.Id,
            UserId = "user",
            UserLanguage = "en",
            Formatting = new NotificationFormatting<string>
            {
                Subject = "Subject"
            }
        };
    }

    private ChannelContext CreateContext(string? configuredToken)
    {
        var user = new User(app.Id, "user", default)
        {
            MobilePushTokens = ReadonlyList.Create(token)
        };

        var configuration = new SendConfiguration();

        if (configuredToken != null)
        {
            configuration["Token"] = configuredToken;
        }

        return new ChannelContext
        {
            App = app,
            AppId = app.Id,
            Configuration = configuration,
            ConfigurationId = Guid.NewGuid(),
            IsUpdate = false,
            Setting = new ChannelSetting(),
            User = user,
            UserId = user.Id
        };
    }
}

// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Notifo.Domain.Apps;
using Notifo.Domain.ChannelTemplates;
using Notifo.Domain.Integrations;
using Notifo.Domain.Log;
using Notifo.Domain.UserNotifications;
using Notifo.Domain.Users;
using Notifo.Infrastructure.Mediator;
using Notifo.Infrastructure.Scheduling;

namespace Notifo.Domain.Channels.Sms;

public class SmsChannelTests
{
    private readonly IAppStore appStore = A.Fake<IAppStore>();
    private readonly IUserNotificationStore userNotificationStore = A.Fake<IUserNotificationStore>();
    private readonly SmsChannel sut;

    public SmsChannelTests()
    {
        var serviceProvider =
            new ServiceCollection()
                .AddSingleton(appStore)
                .AddSingleton(A.Fake<IIntegrationManager>())
                .AddSingleton(userNotificationStore)
                .AddSingleton(A.Fake<IUserStore>())
                .AddSingleton(A.Fake<ILogger<SmsChannel>>())
                .AddSingleton(A.Fake<ILogStore>())
                .AddSingleton(A.Fake<IMediator>())
                .AddSingleton(A.Fake<IScheduler<SmsJob>>())
                .BuildServiceProvider();

        sut = new SmsChannel(serviceProvider, A.Fake<ISmsFormatter>(), A.Fake<IChannelTemplateStore<SmsTemplate>>());
    }

    [Fact]
    public async Task Should_mark_job_as_handled_if_app_has_been_deleted()
    {
        A.CallTo(() => appStore.GetCachedAsync("app", A<CancellationToken>._))
            .Returns((App?)null);

        await sut.HandleAsync([CreateJob()], false, default);

        A.CallTo(() => userNotificationStore.TrackAsync(A<TrackingKey>._, DeliveryResult.Handled, A<CancellationToken>._))
            .MustHaveHappened();
    }

    private static SmsJob CreateJob()
    {
        var notification = new UserNotification
        {
            Id = Guid.NewGuid(),
            AppId = "app",
            UserId = "user",
            UserLanguage = "en",
            Formatting = new NotificationFormatting<string>
            {
                Subject = "Subject"
            }
        };

        var context = new ChannelContext
        {
            App = null!,
            AppId = "app",
            Configuration = [],
            ConfigurationId = Guid.NewGuid(),
            IsUpdate = false,
            Setting = new ChannelSetting(),
            User = null!,
            UserId = "user"
        };

        return new SmsJob(notification, context);
    }
}

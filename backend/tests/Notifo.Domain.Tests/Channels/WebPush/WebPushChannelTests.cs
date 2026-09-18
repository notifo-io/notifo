// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notifo.Domain.Apps;
using Notifo.Domain.Integrations;
using Notifo.Domain.Log;
using Notifo.Domain.UserNotifications;
using Notifo.Domain.Users;
using Notifo.Infrastructure.Json;
using Notifo.Infrastructure.Mediator;
using Notifo.Infrastructure.Scheduling;

namespace Notifo.Domain.Channels.WebPush;

public class WebPushChannelTests
{
    private readonly IAppStore appStore = A.Fake<IAppStore>();
    private readonly IMediator mediator = A.Fake<IMediator>();
    private readonly IUserNotificationStore userNotificationStore = A.Fake<IUserNotificationStore>();
    private readonly App app = new App("app", default);
    private readonly WebPushChannel sut;

    public WebPushChannelTests()
    {
        A.CallTo(() => appStore.GetCachedAsync(app.Id, A<CancellationToken>._))
            .Returns(app);

        var serviceProvider =
            new ServiceCollection()
                .AddSingleton(appStore)
                .AddSingleton(A.Fake<IIntegrationManager>())
                .AddSingleton(userNotificationStore)
                .AddSingleton(A.Fake<IUserStore>())
                .AddSingleton(A.Fake<ILogger<WebPushChannel>>())
                .AddSingleton(A.Fake<ILogStore>())
                .AddSingleton(mediator)
                .AddSingleton(A.Fake<IScheduler<WebPushJob>>())
                .BuildServiceProvider();

        var options = Options.Create(new WebPushOptions
        {
            Subject = "mailto:hello@notifo.io",
            VapidPublicKey = "BAMHB4Q3EKK-uSJzVjFQnJ8CzZMHzjTPMFOFCSzXWuScvMTBLmjBzSLWHBkVZOgZ2vC9m9VLZ7wOEIsSOSCLxEg",
            VapidPrivateKey = "9-WG_x-hm11h5Kj0iGWNPPXgYGzZ0vjCGmFxLRJs0i8"
        });

        sut = new WebPushChannel(serviceProvider, A.Fake<IJsonSerializer>(), options);
    }

    [Fact]
    public async Task Should_remove_subscription_without_keys()
    {
        var job = CreateJob();

        await sut.HandleAsync([job], false, default);

        A.CallTo(() => userNotificationStore.TrackAsync(A<TrackingKey>._, A<DeliveryResult>.That.Matches(x => x.Status == DeliveryStatus.Failed), A<CancellationToken>._))
            .MustHaveHappened();

        A.CallTo(() => mediator.SendAsync<User?>(
                A<RemoveUserWebPushSubscription>.That.Matches(x => x.Endpoint == job.Subscription.Endpoint),
                A<CancellationToken>._))
            .MustHaveHappened();
    }

    private WebPushJob CreateJob()
    {
        var notification = new UserNotification
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

        var context = new ChannelContext
        {
            App = app,
            AppId = app.Id,
            Configuration = [],
            ConfigurationId = Guid.NewGuid(),
            IsUpdate = false,
            Setting = new ChannelSetting(),
            User = null!,
            UserId = "user"
        };

        return new WebPushJob(notification, context, new WebPushSubscription { Endpoint = "https://endpoint" }, A.Fake<IJsonSerializer>());
    }
}

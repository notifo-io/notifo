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

namespace Notifo.Domain.Channels.Messaging;

public class MessagingChannelTests
{
    private readonly IAppStore appStore = A.Fake<IAppStore>();
    private readonly IIntegrationManager integrationManager = A.Fake<IIntegrationManager>();
    private readonly IMessagingFormatter messagingFormatter = A.Fake<IMessagingFormatter>();
    private readonly IChannelTemplateStore<MessagingTemplate> messagingTemplateStore = A.Fake<IChannelTemplateStore<MessagingTemplate>>();
    private readonly IMessagingSender sender = A.Fake<IMessagingSender>();
    private readonly IUserNotificationStore userNotificationStore = A.Fake<IUserNotificationStore>();
    private readonly IUserStore userStore = A.Fake<IUserStore>();
    private readonly App app = new App("app", default);
    private readonly User user = new User("app", "user", default);
    private readonly MessagingChannel sut;

    public MessagingChannelTests()
    {
        A.CallTo(() => appStore.GetCachedAsync(app.Id, A<CancellationToken>._))
            .Returns(app);

        A.CallTo(() => userStore.GetCachedAsync(app.Id, user.Id, A<CancellationToken>._))
            .Returns(user);

        A.CallTo(() => messagingTemplateStore.GetBestAsync(app.Id, A<string?>._, "en", A<CancellationToken>._))
            .Returns((TemplateResolveStatus.Resolved, null));

        A.CallTo(() => messagingFormatter.Format(A<MessagingTemplate?>._, A<MessagingJob>._, app, user))
            .Returns(("Text", null));

        var serviceProvider =
            new ServiceCollection()
                .AddSingleton(appStore)
                .AddSingleton(integrationManager)
                .AddSingleton(userNotificationStore)
                .AddSingleton(userStore)
                .AddSingleton(A.Fake<ILogger<MessagingChannel>>())
                .AddSingleton(A.Fake<ILogStore>())
                .AddSingleton(A.Fake<IMediator>())
                .AddSingleton(A.Fake<IScheduler<MessagingJob>>())
                .BuildServiceProvider();

        sut = new MessagingChannel(serviceProvider, messagingFormatter, messagingTemplateStore);
    }

    [Fact]
    public async Task Should_track_result_of_successful_integration()
    {
        var job = CreateJob();

        var context1 = SetupIntegrations("integration1");

        A.CallTo(() => sender.SendAsync(context1, A<MessagingMessage>._, A<CancellationToken>._))
            .Returns(DeliveryResult.Handled);

        await sut.HandleAsync([job], false, default);

        A.CallTo(() => userNotificationStore.TrackAsync(A<TrackingKey>._, DeliveryResult.Handled, A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_track_skipped_result_of_integration()
    {
        var job = CreateJob();

        var context1 = SetupIntegrations("integration1");

        A.CallTo(() => sender.SendAsync(context1, A<MessagingMessage>._, A<CancellationToken>._))
            .Returns(DeliveryResult.Skipped());

        await sut.HandleAsync([job], false, default);

        A.CallTo(() => userNotificationStore.TrackAsync(A<TrackingKey>._, A<DeliveryResult>.That.Matches(x => x.Status == DeliveryStatus.Skipped), A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_fallback_to_next_integration_of_same_type_if_first_failed()
    {
        var job = CreateJob();

        var (context1, context2) = SetupIntegrations2("integration1", "integration2");

        A.CallTo(() => sender.SendAsync(context1, A<MessagingMessage>._, A<CancellationToken>._))
            .Throws(new InvalidOperationException());

        A.CallTo(() => sender.SendAsync(context2, A<MessagingMessage>._, A<CancellationToken>._))
            .Returns(DeliveryResult.Sent);

        await sut.HandleAsync([job], false, default);

        A.CallTo(() => userNotificationStore.TrackAsync(A<TrackingKey>._, DeliveryResult.Sent, A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_throw_exception_if_last_integration_failed()
    {
        var job = CreateJob();

        var (context1, context2) = SetupIntegrations2("integration1", "integration2");

        A.CallTo(() => sender.SendAsync(A<IntegrationContext>._, A<MessagingMessage>._, A<CancellationToken>._))
            .Throws(new InvalidOperationException());

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.HandleAsync([job], false, default));

        A.CallTo(() => sender.SendAsync(context2, A<MessagingMessage>._, A<CancellationToken>._))
            .MustHaveHappened();
    }

    private IntegrationContext SetupIntegrations(string id)
    {
        var context = CreateContext(id);

        A.CallTo(() => integrationManager.Resolve<IMessagingSender>(app, A<IIntegrationTarget?>._))
            .Returns([new ResolvedIntegration<IMessagingSender>(id, context, sender)]);

        return context;
    }

    private (IntegrationContext, IntegrationContext) SetupIntegrations2(string id1, string id2)
    {
        var context1 = CreateContext(id1);
        var context2 = CreateContext(id2);

        // Integrations of the same type share the same sender instance.
        A.CallTo(() => integrationManager.Resolve<IMessagingSender>(app, A<IIntegrationTarget?>._))
            .Returns(
            [
                new ResolvedIntegration<IMessagingSender>(id1, context1, sender),
                new ResolvedIntegration<IMessagingSender>(id2, context2, sender)
            ]);

        return (context1, context2);
    }

    private IntegrationContext CreateContext(string id)
    {
        return new IntegrationContext
        {
            AppId = app.Id,
            AppName = app.Name,
            CallbackToken = string.Empty,
            CallbackUrl = string.Empty,
            IntegrationAdapter = A.Fake<IIntegrationAdapter>(),
            IntegrationId = id,
            Properties = [],
            UpdateStatusAsync = (_, _) => Task.CompletedTask,
            WebhookUrl = string.Empty
        };
    }

    private MessagingJob CreateJob()
    {
        var notification = new UserNotification
        {
            Id = Guid.NewGuid(),
            AppId = app.Id,
            UserId = user.Id,
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
            User = user,
            UserId = user.Id
        };

        return new MessagingJob(notification, context);
    }
}

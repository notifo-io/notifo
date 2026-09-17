// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Channels.MobilePush;
using Notifo.Domain.Channels.Webhook;
using Notifo.Domain.Channels.WebPush;
using Notifo.Domain.Integrations;
using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure.Json;

namespace Notifo.Domain.Channels;

public class ScheduleKeyTests
{
    private readonly UserNotification notification = new UserNotification
    {
        Id = Guid.NewGuid(),
        AppId = "app",
        UserId = "user",
        Formatting = new NotificationFormatting<string>
        {
            Subject = "Subject"
        }
    };

    [Fact]
    public void Should_create_different_web_push_keys_for_different_subscriptions()
    {
        var serializer = A.Fake<IJsonSerializer>();

        var job1 = new WebPushJob(notification, CreateContext(), new WebPushSubscription { Endpoint = "https://endpoint1" }, serializer);
        var job2 = new WebPushJob(notification, CreateContext(), new WebPushSubscription { Endpoint = "https://endpoint2" }, serializer);

        Assert.NotEqual(job1.ScheduleKey, job2.ScheduleKey);
        Assert.DoesNotContain(".", job1.ScheduleKey, StringComparison.Ordinal);
    }

    [Fact]
    public void Should_create_different_webhook_keys_for_different_integrations()
    {
        var job1 = new WebhookJob(notification, CreateContext(), "integration1");
        var job2 = new WebhookJob(notification, CreateContext(), "integration2");

        Assert.NotEqual(job1.ScheduleKey, job2.ScheduleKey);
    }

    [Fact]
    public void Should_create_different_mobile_push_keys_for_wakeup_and_notification_with_group_key()
    {
        var context = CreateContext();

        context.Setting.GroupKey = "group";

        var token = new MobilePushToken
        {
            Token = "token",
            DeviceType = MobileDeviceType.iOS
        };

        var wakeupNotification = new UserNotification
        {
            AppId = notification.AppId,
            UserId = notification.UserId
        };

        var job = new MobilePushJob(notification, context, token);
        var wakeupJob = new MobilePushJob(wakeupNotification, context, token);

        Assert.NotEqual(job.ScheduleKey, wakeupJob.ScheduleKey);
    }

    private static ChannelContext CreateContext()
    {
        return new ChannelContext
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
    }
}

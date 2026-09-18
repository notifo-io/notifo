// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Domain.Channels;
using Notifo.Domain.Integrations;
using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure;

namespace Notifo.Domain.Shared;

public abstract class UserNotificationRepositoryTests
{
    private readonly Guid configurationId1 = Guid.NewGuid();
    private readonly Guid configurationId2 = Guid.NewGuid();
    private readonly string userId2 = Guid.NewGuid().ToString();

    // Notifications expire after the retention time, therefore the timestamp cannot be fixed.
    protected Instant Now { get; } = Instant.FromUnixTimeMilliseconds(SystemClock.Instance.GetCurrentInstant().ToUnixTimeMilliseconds());

    protected string AppId { get; } = Guid.NewGuid().ToString();

    protected string Channel { get; } = "webpush";

    protected string Configuration1 { get; } = Guid.NewGuid().ToString();

    protected string Configuration2 { get; } = Guid.NewGuid().ToString();

    protected string UserId1 { get; } = Guid.NewGuid().ToString();

    protected abstract Task<IUserNotificationRepository> CreateSutAsync();

    [Fact]
    public async Task Should_store_notification()
    {
        var sut = await CreateSutAsync();

        var notification1 = CreateNotification(UserId1);
        var notification2 = CreateNotification(userId2);

        await sut.InsertAsync(notification1, default);
        await sut.InsertAsync(notification2, default);

        var notifications1 = await sut.QueryAsync(AppId, UserId1, new UserNotificationQuery(), default);
        var notifications2 = await sut.QueryAsync(AppId, userId2, new UserNotificationQuery(), default);

        notifications1.ToArray().Should().BeEquivalentTo(new[] { notification1 });
        notifications2.ToArray().Should().BeEquivalentTo(new[] { notification2 });
    }

    [Fact]
    public async Task Should_cleanup_old_notifications()
    {
        var sut = await CreateSutAsync();

        var time = Now;

        for (var i = 0; i < 200; i++)
        {
            await sut.InsertAsync(CreateNotification(UserId1, time), default);

            time = time.Plus(Duration.FromSeconds(1));
        }

        var notifications = await sut.QueryAsync(AppId, UserId1, new UserNotificationQuery { TotalNeeded = true }, default);

        Assert.Equal(100, notifications.Total);
    }

    [Fact]
    public async Task Should_update_status()
    {
        var sut = await CreateSutAsync();

        var notification1 = CreateNotification(UserId1);
        var notification2 = CreateNotification(userId2);

        await sut.InsertAsync(notification1, default);
        await sut.InsertAsync(notification2, default);

        var result = new DeliveryResult(DeliveryStatus.Handled, "Update Details");

        await sut.BatchWriteAsync(
        [
            (new TrackingToken(notification1.Id, Channel, configurationId1), result),
            (new TrackingToken(notification1.Id, Channel, configurationId2), result),
            (new TrackingToken(notification2.Id, Channel, configurationId1), result),
            (new TrackingToken(notification2.Id, Channel, configurationId2), result),
        ], Now, default);

        UpdateStatus(notification1, Channel, configurationId1, result);
        UpdateStatus(notification1, Channel, configurationId2, result);
        UpdateStatus(notification2, Channel, configurationId1, result);
        UpdateStatus(notification2, Channel, configurationId2, result);

        var notifications1 = await sut.QueryAsync(AppId, UserId1, new UserNotificationQuery(), default);
        var notifications2 = await sut.QueryAsync(AppId, userId2, new UserNotificationQuery(), default);

        notifications1.ToArray().Should().BeEquivalentTo(new[] { notification1 });
        notifications2.ToArray().Should().BeEquivalentTo(new[] { notification2 });
    }

    [Fact]
    public async Task Should_update_status_with_configuration_string()
    {
        var sut = await CreateSutAsync();

        var notification1 = CreateNotification(UserId1);
        var notification2 = CreateNotification(userId2);

        await sut.InsertAsync(notification1, default);
        await sut.InsertAsync(notification2, default);

        var result = new DeliveryResult(DeliveryStatus.Handled, "Update Details");

        await sut.BatchWriteAsync(
        [
            (new TrackingToken(notification1.Id, Channel, default, Configuration1), result),
            (new TrackingToken(notification1.Id, Channel, default, Configuration2), result),
            (new TrackingToken(notification2.Id, Channel, default, Configuration1), result),
            (new TrackingToken(notification2.Id, Channel, default, Configuration2), result),
        ], Now, default);

        UpdateStatus(notification1, Channel, configurationId1, result);
        UpdateStatus(notification1, Channel, configurationId2, result);
        UpdateStatus(notification2, Channel, configurationId1, result);
        UpdateStatus(notification2, Channel, configurationId2, result);

        var notifications1 = await sut.QueryAsync(AppId, UserId1, new UserNotificationQuery(), default);
        var notifications2 = await sut.QueryAsync(AppId, userId2, new UserNotificationQuery(), default);

        notifications1.ToArray().Should().BeEquivalentTo(new[] { notification1 });
        notifications2.ToArray().Should().BeEquivalentTo(new[] { notification2 });
    }

    [Fact]
    public async Task Should_not_update_status_without_channel()
    {
        var sut = await CreateSutAsync();

        var notification1 = CreateNotification(UserId1);
        var notification2 = CreateNotification(userId2);

        await sut.InsertAsync(notification1, default);
        await sut.InsertAsync(notification2, default);

        var result = new DeliveryResult(DeliveryStatus.Handled, "Update Details");

        await sut.BatchWriteAsync(
        [
            (new TrackingToken(notification1.Id), result),
            (new TrackingToken(notification2.Id), result),
        ], Now, default);

        var notifications1 = await sut.QueryAsync(AppId, UserId1, new UserNotificationQuery(), default);
        var notifications2 = await sut.QueryAsync(AppId, userId2, new UserNotificationQuery(), default);

        notifications1.ToArray().Should().BeEquivalentTo(new[] { notification1 });
        notifications2.ToArray().Should().BeEquivalentTo(new[] { notification2 });
    }

    [Fact]
    public async Task Should_not_update_status_without_configuration()
    {
        var sut = await CreateSutAsync();

        var notification1 = CreateNotification(UserId1);
        var notification2 = CreateNotification(userId2);

        await sut.InsertAsync(notification1, default);
        await sut.InsertAsync(notification2, default);

        var result = new DeliveryResult(DeliveryStatus.Handled, "Update Details");

        await sut.BatchWriteAsync(
        [
            (new TrackingToken(notification1.Id, Channel), result),
            (new TrackingToken(notification2.Id, Channel), result),
        ], Now, default);

        var notifications1 = await sut.QueryAsync(AppId, UserId1, new UserNotificationQuery(), default);
        var notifications2 = await sut.QueryAsync(AppId, userId2, new UserNotificationQuery(), default);

        notifications1.ToArray().Should().BeEquivalentTo(new[] { notification1 });
        notifications2.ToArray().Should().BeEquivalentTo(new[] { notification2 });
    }

    [Fact]
    public async Task Should_mark_as_delivered()
    {
        var sut = await CreateSutAsync();

        var notification = CreateNotification(UserId1);

        await sut.InsertAsync(notification, default);
        await sut.TrackDeliveredAsync([new TrackingToken(notification.Id, Channel, configurationId1)], Now, default);

        var info = new HandledInfo(Now, Channel);

        notification.Channels[Channel].Status[configurationId1].FirstDelivered = Now;
        notification.Channels[Channel].FirstDelivered = Now;
        notification.FirstDelivered = info;

        var notifications1 = await sut.QueryAsync(AppId, UserId1, new UserNotificationQuery(), default);

        notifications1.ToArray().Should().BeEquivalentTo(new[] { notification });
    }

    [Fact]
    public async Task Should_mark_as_delivered_with_configuration()
    {
        var sut = await CreateSutAsync();

        var notification = CreateNotification(UserId1);

        await sut.InsertAsync(notification, default);
        await sut.TrackDeliveredAsync([new TrackingToken(notification.Id, Channel, default, Configuration1)], Now, default);

        var info = new HandledInfo(Now, Channel);

        notification.Channels[Channel].Status[configurationId1].FirstDelivered = Now;
        notification.Channels[Channel].FirstDelivered = Now;
        notification.FirstDelivered = info;

        var notifications1 = await sut.QueryAsync(AppId, UserId1, new UserNotificationQuery(), default);

        notifications1.ToArray().Should().BeEquivalentTo(new[] { notification });
    }

    [Fact]
    public async Task Should_mark_as_delivered_without_configuration_id()
    {
        var sut = await CreateSutAsync();

        var notification = CreateNotification(UserId1);

        await sut.InsertAsync(notification, default);
        await sut.TrackDeliveredAsync([new TrackingToken(notification.Id, Channel)], Now, default);

        var info = new HandledInfo(Now, Channel);

        notification.Channels[Channel].FirstDelivered = Now;
        notification.FirstDelivered = info;

        var notifications1 = await sut.QueryAsync(AppId, UserId1, new UserNotificationQuery(), default);

        notifications1.ToArray().Should().BeEquivalentTo(new[] { notification });
    }

    [Fact]
    public async Task Should_mark_as_delivered_without_channel()
    {
        var sut = await CreateSutAsync();

        var notification = CreateNotification(UserId1);

        await sut.InsertAsync(notification, default);
        await sut.TrackDeliveredAsync([new TrackingToken(notification.Id)], Now, default);

        var info = new HandledInfo(Now, null);

        notification.FirstDelivered = info;

        var notifications1 = await sut.QueryAsync(AppId, UserId1, new UserNotificationQuery(), default);

        notifications1.ToArray().Should().BeEquivalentTo(new[] { notification });
    }

    [Fact]
    public async Task Should_mark_as_seen()
    {
        var sut = await CreateSutAsync();

        var notification = CreateNotification(UserId1);

        await sut.InsertAsync(notification, default);
        await sut.TrackSeenAsync([new TrackingToken(notification.Id, Channel, configurationId1)], Now, default);

        var info = new HandledInfo(Now, Channel);

        notification.Channels[Channel].Status[configurationId1].FirstSeen = Now;
        notification.Channels[Channel].Status[configurationId1].FirstDelivered = Now;
        notification.Channels[Channel].FirstSeen = Now;
        notification.Channels[Channel].FirstDelivered = Now;
        notification.FirstSeen = info;
        notification.FirstDelivered = info;

        var notifications1 = await sut.QueryAsync(AppId, UserId1, new UserNotificationQuery(), default);

        notifications1.ToArray().Should().BeEquivalentTo(new[] { notification });
    }

    [Fact]
    public async Task Should_mark_as_seen_with_configuration()
    {
        var sut = await CreateSutAsync();

        var notification = CreateNotification(UserId1);

        await sut.InsertAsync(notification, default);
        await sut.TrackSeenAsync([new TrackingToken(notification.Id, Channel, default, Configuration1)], Now, default);

        var info = new HandledInfo(Now, Channel);

        notification.Channels[Channel].Status[configurationId1].FirstSeen = Now;
        notification.Channels[Channel].Status[configurationId1].FirstDelivered = Now;
        notification.Channels[Channel].FirstSeen = Now;
        notification.Channels[Channel].FirstDelivered = Now;
        notification.FirstSeen = info;
        notification.FirstDelivered = info;

        var notifications1 = await sut.QueryAsync(AppId, UserId1, new UserNotificationQuery(), default);

        notifications1.ToArray().Should().BeEquivalentTo(new[] { notification });
    }

    [Fact]
    public async Task Should_mark_as_seen_without_configuration_id()
    {
        var sut = await CreateSutAsync();

        var notification = CreateNotification(UserId1);

        await sut.InsertAsync(notification, default);
        await sut.TrackSeenAsync([new TrackingToken(notification.Id, Channel)], Now, default);

        var info = new HandledInfo(Now, Channel);

        notification.Channels[Channel].FirstSeen = Now;
        notification.Channels[Channel].FirstDelivered = Now;
        notification.FirstSeen = info;
        notification.FirstDelivered = info;

        var notifications1 = await sut.QueryAsync(AppId, UserId1, new UserNotificationQuery(), default);

        notifications1.ToArray().Should().BeEquivalentTo(new[] { notification });
    }

    [Fact]
    public async Task Should_mark_as_seen_without_channel()
    {
        var sut = await CreateSutAsync();

        var notification = CreateNotification(UserId1);

        await sut.InsertAsync(notification, default);
        await sut.TrackSeenAsync([new TrackingToken(notification.Id)], Now, default);

        var info = new HandledInfo(Now, null);

        notification.FirstSeen = info;
        notification.FirstDelivered = info;

        var notifications1 = await sut.QueryAsync(AppId, UserId1, new UserNotificationQuery(), default);

        notifications1.ToArray().Should().BeEquivalentTo(new[] { notification });
    }

    [Fact]
    public async Task Should_not_mark_as_confirmed_if_confirm_mode_not_set()
    {
        var sut = await CreateSutAsync();

        var notification = CreateNotification(UserId1);

        notification.Formatting.ConfirmMode = ConfirmMode.None;

        await sut.InsertAsync(notification, default);
        await sut.TrackConfirmedAsync([new TrackingToken(notification.Id, Channel, configurationId1)], Now, default);

        var info = new HandledInfo(Now, Channel);

        notification.Channels[Channel].Status[configurationId1].FirstSeen = Now;
        notification.Channels[Channel].Status[configurationId1].FirstDelivered = Now;
        notification.Channels[Channel].FirstSeen = Now;
        notification.Channels[Channel].FirstDelivered = Now;
        notification.FirstSeen = info;
        notification.FirstDelivered = info;

        var notifications1 = await sut.QueryAsync(AppId, UserId1, new UserNotificationQuery(), default);

        notifications1.ToArray().Should().BeEquivalentTo(new[] { notification });
    }

    [Fact]
    public async Task Should_mark_as_confirmed()
    {
        var sut = await CreateSutAsync();

        var notification = CreateNotification(UserId1);

        await sut.InsertAsync(notification, default);
        await sut.TrackConfirmedAsync([new TrackingToken(notification.Id, Channel, configurationId1)], Now, default);

        var info = new HandledInfo(Now, Channel);

        notification.Updated = Now;
        notification.Channels[Channel].Status[configurationId1].FirstConfirmed = Now;
        notification.Channels[Channel].Status[configurationId1].FirstSeen = Now;
        notification.Channels[Channel].Status[configurationId1].FirstDelivered = Now;
        notification.Channels[Channel].FirstConfirmed = Now;
        notification.Channels[Channel].FirstSeen = Now;
        notification.Channels[Channel].FirstDelivered = Now;
        notification.FirstConfirmed = info;
        notification.FirstSeen = info;
        notification.FirstDelivered = info;

        var notifications1 = await sut.QueryAsync(AppId, UserId1, new UserNotificationQuery(), default);

        notifications1.ToArray().Should().BeEquivalentTo(new[] { notification });
    }

    [Fact]
    public async Task Should_mark_as_confirmed_with_configuration()
    {
        var sut = await CreateSutAsync();

        var notification = CreateNotification(UserId1);

        await sut.InsertAsync(notification, default);
        await sut.TrackConfirmedAsync([new TrackingToken(notification.Id, Channel, default, Configuration1)], Now, default);

        var info = new HandledInfo(Now, Channel);

        notification.Updated = Now;
        notification.Channels[Channel].Status[configurationId1].FirstConfirmed = Now;
        notification.Channels[Channel].Status[configurationId1].FirstSeen = Now;
        notification.Channels[Channel].Status[configurationId1].FirstDelivered = Now;
        notification.Channels[Channel].FirstConfirmed = Now;
        notification.Channels[Channel].FirstSeen = Now;
        notification.Channels[Channel].FirstDelivered = Now;
        notification.FirstConfirmed = info;
        notification.FirstSeen = info;
        notification.FirstDelivered = info;

        var notifications1 = await sut.QueryAsync(AppId, UserId1, new UserNotificationQuery(), default);

        notifications1.ToArray().Should().BeEquivalentTo(new[] { notification });
    }

    [Fact]
    public async Task Should_mark_as_confirmed_without_configuration_id()
    {
        var sut = await CreateSutAsync();

        var notification = CreateNotification(UserId1);

        await sut.InsertAsync(notification, default);
        await sut.TrackConfirmedAsync([new TrackingToken(notification.Id, Channel)], Now, default);

        var info = new HandledInfo(Now, Channel);

        notification.Updated = Now;
        notification.Channels[Channel].FirstConfirmed = Now;
        notification.Channels[Channel].FirstSeen = Now;
        notification.Channels[Channel].FirstDelivered = Now;
        notification.FirstConfirmed = info;
        notification.FirstSeen = info;
        notification.FirstDelivered = info;

        var notifications1 = await sut.QueryAsync(AppId, UserId1, new UserNotificationQuery(), default);

        notifications1.ToArray().Should().BeEquivalentTo(new[] { notification });
    }

    [Fact]
    public async Task Should_mark_as_confirmed_without_channel()
    {
        var sut = await CreateSutAsync();

        var notification = CreateNotification(UserId1);

        await sut.InsertAsync(notification, default);
        await sut.TrackConfirmedAsync([new TrackingToken(notification.Id)], Now, default);

        var info = new HandledInfo(Now, null);

        notification.Updated = Now;
        notification.FirstConfirmed = info;
        notification.FirstSeen = info;
        notification.FirstDelivered = info;

        var notifications1 = await sut.QueryAsync(AppId, UserId1, new UserNotificationQuery(), default);

        notifications1.ToArray().Should().BeEquivalentTo(new[] { notification });
    }

    [Fact]
    public async Task Should_detect_handled_notification()
    {
        var sut = await CreateSutAsync();

        var notification = CreateNotification(UserId1);

        await sut.InsertAsync(notification, default);

        var isHandledBefore = await sut.IsHandledAsync(notification.Id, Channel, configurationId1, default);

        await sut.BatchWriteAsync(
        [
            (new TrackingToken(notification.Id, Channel, configurationId1), DeliveryResult.Handled)
        ], Now, default);

        var isHandledAfter = await sut.IsHandledAsync(notification.Id, Channel, configurationId1, default);

        Assert.False(isHandledBefore);
        Assert.True(isHandledAfter);
    }

    [Fact]
    public async Task Should_query_notifications_after_timestamp_sorted_by_update()
    {
        var sut = await CreateSutAsync();

        // The newest notification by created time is the oldest by updated time.
        var notification1 = CreateNotification(userId2, Now.Plus(Duration.FromMinutes(3)));
        var notification2 = CreateNotification(userId2, Now.Plus(Duration.FromMinutes(2)));
        var notification3 = CreateNotification(userId2, Now.Plus(Duration.FromMinutes(1)));

        notification1.Updated = Now.Plus(Duration.FromMinutes(1));
        notification2.Updated = Now.Plus(Duration.FromMinutes(2));
        notification3.Updated = Now.Plus(Duration.FromMinutes(3));

        await sut.InsertAsync(notification1, default);
        await sut.InsertAsync(notification2, default);
        await sut.InsertAsync(notification3, default);

        var notifications = await sut.QueryAsync(AppId, userId2, new UserNotificationQuery { After = Now }, default);

        Assert.Equal(
            [notification1.Id, notification2.Id, notification3.Id],
            notifications.Select(x => x.Id).ToArray());
    }

    [Fact]
    public async Task Should_query_notification_with_correlation_id()
    {
        var sut = await CreateSutAsync();

        var notification = CreateNotification(UserId1);

        notification.CorrelationId = Guid.NewGuid().ToString();

        await sut.InsertAsync(notification, default);

        var notifications = await sut.QueryAsync(AppId, new UserNotificationQuery(), default);

        Assert.Contains(notifications, x => x.Id == notification.Id);
    }

    [Fact]
    public async Task Should_not_mark_as_updated_if_already_seen()
    {
        var sut = await CreateSutAsync();

        var notification = CreateNotification(UserId1);

        await sut.InsertAsync(notification, default);

        var tokens = new[] { new TrackingToken(notification.Id, Channel, configurationId1) };

        var result1 = await sut.TrackSeenAsync(tokens, Now, default);
        var result2 = await sut.TrackSeenAsync(tokens, Now.Plus(Duration.FromMinutes(1)), default);

        Assert.True(result1.Single().Item2);
        Assert.False(result2.Single().Item2);
    }

    [Fact]
    public async Task Should_find_notification_by_id()
    {
        var sut = await CreateSutAsync();

        var notification = CreateNotification(UserId1);

        await sut.InsertAsync(notification);

        var result = await sut.FindAsync(notification.Id);

        result.Should().BeEquivalentTo(notification);
    }

    [Fact]
    public async Task Should_return_null_if_notification_not_found()
    {
        var sut = await CreateSutAsync();

        var result = await sut.FindAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task Should_throw_exception_if_notification_already_exists()
    {
        var sut = await CreateSutAsync();

        var notification = CreateNotification(UserId1);

        await sut.InsertAsync(notification);

        await Assert.ThrowsAsync<UniqueConstraintException>(() => sut.InsertAsync(notification));
    }

    [Fact]
    public async Task Should_mark_notification_as_deleted()
    {
        var sut = await CreateSutAsync();

        var notification = CreateNotification(UserId1);

        await sut.InsertAsync(notification);
        await sut.DeleteAsync(notification.Id);

        var result = await sut.FindAsync(notification.Id);

        Assert.True(result!.IsDeleted);
    }

    [Fact]
    public async Task Should_query_notifications_by_scope()
    {
        var sut = await CreateSutAsync();

        var deleted = CreateNotification(UserId1);
        var nonDeleted = CreateNotification(UserId1);

        await sut.InsertAsync(deleted);
        await sut.InsertAsync(nonDeleted);
        await sut.DeleteAsync(deleted.Id);

        var resultDefault = await sut.QueryAsync(AppId, UserId1, new UserNotificationQuery());
        var resultNonDeleted = await sut.QueryAsync(AppId, UserId1, new UserNotificationQuery { Scope = UserNotificationQueryScope.NonDeleted });
        var resultDeleted = await sut.QueryAsync(AppId, UserId1, new UserNotificationQuery { Scope = UserNotificationQueryScope.Deleted });
        var resultAll = await sut.QueryAsync(AppId, UserId1, new UserNotificationQuery { Scope = UserNotificationQueryScope.All });

        Assert.Equal([nonDeleted.Id], resultDefault.Select(x => x.Id));
        Assert.Equal([nonDeleted.Id], resultNonDeleted.Select(x => x.Id));
        Assert.Equal([deleted.Id], resultDeleted.Select(x => x.Id));
        Assert.Equal(new[] { deleted.Id, nonDeleted.Id }.Order(), resultAll.Select(x => x.Id).Order());
    }

    [Fact]
    public async Task Should_query_notifications_of_user_sorted_by_created()
    {
        var sut = await CreateSutAsync();

        var notification1 = CreateNotification(UserId1, Now.Plus(Duration.FromMinutes(1)));
        var notification2 = CreateNotification(UserId1, Now.Plus(Duration.FromMinutes(3)));
        var notification3 = CreateNotification(UserId1, Now.Plus(Duration.FromMinutes(2)));

        await sut.InsertAsync(notification1);
        await sut.InsertAsync(notification2);
        await sut.InsertAsync(notification3);

        var result = await sut.QueryAsync(AppId, UserId1, new UserNotificationQuery());

        Assert.Equal([notification2.Id, notification3.Id, notification1.Id], result.Select(x => x.Id));
    }

    [Fact]
    public async Task Should_query_notifications_of_user_by_subject()
    {
        var sut = await CreateSutAsync();

        var notification1 = CreateNotification(UserId1);
        var notification2 = CreateNotification(UserId1);

        notification1.Formatting.Subject = "Hello World";
        notification2.Formatting.Subject = "Goodbye";

        await sut.InsertAsync(notification1);
        await sut.InsertAsync(notification2);

        var result = await sut.QueryAsync(AppId, UserId1, new UserNotificationQuery { Query = "WORLD" });

        Assert.Equal([notification1.Id], result.Select(x => x.Id));
    }

    [Fact]
    public async Task Should_query_notifications_of_user_by_channels()
    {
        var sut = await CreateSutAsync();

        var notification1 = CreateNotification(UserId1);
        var notification2 = CreateNotification(UserId1);

        notification2.Channels[Channel].Setting.Send = ChannelSend.NotSending;

        await sut.InsertAsync(notification1);
        await sut.InsertAsync(notification2);

        var result = await sut.QueryAsync(AppId, UserId1, new UserNotificationQuery { Channels = [Channel, Providers.Email] });

        Assert.Equal([notification1.Id], result.Select(x => x.Id));
    }

    [Fact]
    public async Task Should_query_notifications_of_user_by_correlation_id()
    {
        var sut = await CreateSutAsync();

        var notification1 = CreateNotification(UserId1);
        var notification2 = CreateNotification(UserId1);

        notification1.CorrelationId = Guid.NewGuid().ToString();

        await sut.InsertAsync(notification1);
        await sut.InsertAsync(notification2);

        var result = await sut.QueryAsync(AppId, UserId1, new UserNotificationQuery { CorrelationId = notification1.CorrelationId });

        Assert.Equal([notification1.Id], result.Select(x => x.Id));
    }

    [Fact]
    public async Task Should_query_notifications_of_user_with_paging_and_total()
    {
        var sut = await CreateSutAsync();

        await sut.InsertAsync(CreateNotification(UserId1));
        await sut.InsertAsync(CreateNotification(UserId1));
        await sut.InsertAsync(CreateNotification(UserId1));

        var result = await sut.QueryAsync(AppId, UserId1, new UserNotificationQuery { Take = 2, TotalNeeded = true });

        Assert.Equal(2, result.Count);
        Assert.Equal(3, result.Total);
    }

    [Fact]
    public async Task Should_not_query_total_if_not_needed()
    {
        var sut = await CreateSutAsync();

        await sut.InsertAsync(CreateNotification(UserId1));
        await sut.InsertAsync(CreateNotification(UserId1));
        await sut.InsertAsync(CreateNotification(UserId1));

        var result = await sut.QueryAsync(AppId, UserId1, new UserNotificationQuery { Take = 2 });

        Assert.Equal(2, result.Count);
        Assert.Equal(2, result.Total);
    }

    [Fact]
    public async Task Should_not_query_notifications_of_other_user_or_app()
    {
        var sut = await CreateSutAsync();

        var notification = CreateNotification(UserId1);

        notification.AppId = Guid.NewGuid().ToString();

        await sut.InsertAsync(notification);
        await sut.InsertAsync(CreateNotification(userId2));

        var result = await sut.QueryAsync(AppId, UserId1, new UserNotificationQuery());

        Assert.Empty(result);
    }

    [Fact]
    public async Task Should_query_notifications_of_app_by_correlation_id()
    {
        var sut = await CreateSutAsync();

        var notification1 = CreateNotification(UserId1);
        var notification2 = CreateNotification(userId2);
        var notification3 = CreateNotification(userId2);

        notification1.CorrelationId = Guid.NewGuid().ToString();
        notification2.CorrelationId = notification1.CorrelationId;

        await sut.InsertAsync(notification1);
        await sut.InsertAsync(notification2);
        await sut.InsertAsync(notification3);

        var result = await sut.QueryAsync(AppId, new UserNotificationQuery { CorrelationId = notification1.CorrelationId });

        Assert.Equal(new[] { notification1.Id, notification2.Id }.Order(), result.Select(x => x.Id).Order());
    }

    [Fact]
    public async Task Should_query_notifications_of_app_sorted_by_created()
    {
        var sut = await CreateSutAsync();

        var notification1 = CreateNotification(UserId1, Now.Plus(Duration.FromMinutes(1)));
        var notification2 = CreateNotification(userId2, Now.Plus(Duration.FromMinutes(3)));
        var notification3 = CreateNotification(UserId1, Now.Plus(Duration.FromMinutes(2)));

        await sut.InsertAsync(notification1);
        await sut.InsertAsync(notification2);
        await sut.InsertAsync(notification3);

        var result = await sut.QueryAsync(AppId, new UserNotificationQuery());

        Assert.Equal([notification2.Id, notification3.Id, notification1.Id], result.Select(x => x.Id));
    }

    [Fact]
    public async Task Should_query_notifications_of_app_with_paging_and_total()
    {
        var sut = await CreateSutAsync();

        await sut.InsertAsync(CreateNotification(UserId1));
        await sut.InsertAsync(CreateNotification(userId2));
        await sut.InsertAsync(CreateNotification(userId2));

        var result = await sut.QueryAsync(AppId, new UserNotificationQuery { Take = 2, TotalNeeded = true });

        Assert.Equal(2, result.Count);
        Assert.Equal(3, result.Total);
    }

    [Fact]
    public async Task Should_not_query_notifications_of_other_app()
    {
        var sut = await CreateSutAsync();

        var notification = CreateNotification(UserId1);

        notification.AppId = Guid.NewGuid().ToString();

        await sut.InsertAsync(notification);

        var result = await sut.QueryAsync(AppId, new UserNotificationQuery());

        Assert.Empty(result);
    }

    [Fact]
    public async Task Should_query_last_notification_per_user()
    {
        var sut = await CreateSutAsync();

        var latest = Now.Plus(Duration.FromMinutes(2));

        await sut.InsertAsync(CreateNotification(UserId1, Now.Plus(Duration.FromMinutes(1))));
        await sut.InsertAsync(CreateNotification(UserId1, latest));
        await sut.InsertAsync(CreateNotification(userId2, Now));

        var unknownUser = Guid.NewGuid().ToString();

        var result = await sut.QueryLastNotificationsAsync(AppId, [UserId1, userId2, unknownUser]);

        Assert.Equal(latest, result[UserId1]);
        Assert.Equal(Now, result[userId2]);
        Assert.False(result.ContainsKey(unknownUser));
    }

    [Fact]
    public async Task Should_not_query_deleted_notification_as_last_notification()
    {
        var sut = await CreateSutAsync();

        var notification1 = CreateNotification(UserId1, Now);
        var notification2 = CreateNotification(UserId1, Now.Plus(Duration.FromMinutes(1)));

        await sut.InsertAsync(notification1);
        await sut.InsertAsync(notification2);
        await sut.DeleteAsync(notification2.Id);

        var result = await sut.QueryLastNotificationsAsync(AppId, [UserId1]);

        Assert.Equal(Now, result[UserId1]);
    }

    [Fact]
    public async Task Should_not_detect_handled_notification_for_other_configuration()
    {
        var sut = await CreateSutAsync();

        var notification = CreateNotification(UserId1);

        await sut.InsertAsync(notification);

        await sut.BatchWriteAsync(
        [
            (new TrackingToken(notification.Id, Channel, configurationId1), DeliveryResult.Handled)
        ], Now);

        var result = await sut.IsHandledAsync(notification.Id, Channel, configurationId2);

        Assert.False(result);
    }

    [Fact]
    public async Task Should_not_detect_handled_notification_if_not_found()
    {
        var sut = await CreateSutAsync();

        var result = await sut.IsHandledAsync(Guid.NewGuid(), Channel, configurationId1);

        Assert.False(result);
    }

    [Fact]
    public async Task Should_detect_handled_or_seen_notification_if_seen()
    {
        var sut = await CreateSutAsync();

        var notification = CreateNotification(UserId1);

        await sut.InsertAsync(notification);

        var resultBefore = await sut.IsHandledOrSeenAsync(notification.Id, Channel, configurationId1);

        await sut.TrackSeenAsync([new TrackingToken(notification.Id)], Now);

        var resultAfter = await sut.IsHandledOrSeenAsync(notification.Id, Channel, configurationId1);

        Assert.False(resultBefore);
        Assert.True(resultAfter);
    }

    [Fact]
    public async Task Should_detect_handled_or_seen_notification_if_handled()
    {
        var sut = await CreateSutAsync();

        var notification = CreateNotification(UserId1);

        await sut.InsertAsync(notification);

        await sut.BatchWriteAsync(
        [
            (new TrackingToken(notification.Id, Channel, configurationId1), DeliveryResult.Handled)
        ], Now);

        var result = await sut.IsHandledOrSeenAsync(notification.Id, Channel, configurationId1);

        Assert.True(result);
    }

    [Fact]
    public async Task Should_detect_handled_or_confirmed_notification_if_confirmed()
    {
        var sut = await CreateSutAsync();

        var notification = CreateNotification(UserId1);

        await sut.InsertAsync(notification);

        var resultBefore = await sut.IsHandledOrConfirmedAsync(notification.Id, Channel, configurationId1);

        await sut.TrackConfirmedAsync([new TrackingToken(notification.Id)], Now);

        var resultAfter = await sut.IsHandledOrConfirmedAsync(notification.Id, Channel, configurationId1);

        Assert.False(resultBefore);
        Assert.True(resultAfter);
    }

    [Fact]
    public async Task Should_not_detect_handled_or_confirmed_notification_if_only_seen()
    {
        var sut = await CreateSutAsync();

        var notification = CreateNotification(UserId1);

        await sut.InsertAsync(notification);
        await sut.TrackSeenAsync([new TrackingToken(notification.Id)], Now);

        var result = await sut.IsHandledOrConfirmedAsync(notification.Id, Channel, configurationId1);

        Assert.False(result);
    }

    [Fact]
    public async Task Should_detect_handled_or_confirmed_notification_if_handled()
    {
        var sut = await CreateSutAsync();

        var notification = CreateNotification(UserId1);

        await sut.InsertAsync(notification);

        await sut.BatchWriteAsync(
        [
            (new TrackingToken(notification.Id, Channel, configurationId1), DeliveryResult.Handled)
        ], Now);

        var result = await sut.IsHandledOrConfirmedAsync(notification.Id, Channel, configurationId1);

        Assert.True(result);
    }

    [Fact]
    public async Task Should_return_tracked_notifications()
    {
        var sut = await CreateSutAsync();

        var notification = CreateNotification(UserId1);

        await sut.InsertAsync(notification);

        var result = await sut.TrackDeliveredAsync([new TrackingToken(notification.Id), new TrackingToken(Guid.NewGuid())], Now);

        var (tracked, updated) = Assert.Single(result);

        Assert.Equal(notification.Id, tracked.Id);
        Assert.Equal(new HandledInfo(Now, null), tracked.FirstDelivered);
        Assert.True(updated);
    }

    [Fact]
    public async Task Should_not_fail_if_tracked_notification_not_found()
    {
        var sut = await CreateSutAsync();

        var token = new TrackingToken(Guid.NewGuid(), Channel, configurationId1);

        await sut.BatchWriteAsync([(token, DeliveryResult.Handled)], Now);

        var resultDelivered = await sut.TrackDeliveredAsync([token], Now);
        var resultSeen = await sut.TrackSeenAsync([token], Now);
        var resultConfirmed = await sut.TrackConfirmedAsync([token], Now);

        Assert.Empty(resultDelivered);
        Assert.Empty(resultSeen);
        Assert.Empty(resultConfirmed);
    }

    protected UserNotification CreateNotification(string userId, Instant created = default)
    {
        return new UserNotification
        {
            Id = Guid.NewGuid(),
            AppId = AppId,
            Channels = new Dictionary<string, UserNotificationChannel>
            {
                [Channel] = new UserNotificationChannel
                {
                    Setting = new ChannelSetting
                    {
                        Send = ChannelSend.Send
                    },
                    Status = new Dictionary<Guid, ChannelSendInfo>
                    {
                        [configurationId1] = new ChannelSendInfo
                        {
                            Configuration = new SendConfiguration
                            {
                                ["key1"] = Configuration1
                            }
                        },
                        [configurationId2] = new ChannelSendInfo
                        {
                            Configuration = new SendConfiguration
                            {
                                ["key2"] = Configuration2
                            }
                        }
                    }
                }
            },
            Formatting = new NotificationFormatting<string>()
            {
                ConfirmMode = ConfirmMode.Explicit
            },
            UserId = userId,
            UserLanguage = "en",
            Created = created == default ? Now : created
        };
    }

    private void UpdateStatus(UserNotification notification, string channel, Guid configurationId, DeliveryResult result)
    {
        var statusItem = notification.Channels[channel].Status[configurationId];

        statusItem.LastUpdate = Now;
        statusItem.Status = result.Status;
        statusItem.Detail = result.Detail;
    }
}

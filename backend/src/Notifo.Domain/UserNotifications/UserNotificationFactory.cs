// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Domain.Apps;
using Notifo.Domain.Log;
using Notifo.Domain.UserEvents;
using Notifo.Domain.Users;
using Notifo.Domain.Utils;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Domain.UserNotifications;

public sealed class UserNotificationFactory : IUserNotificationFactory
{
    private const string DefaultConfirmText = "Confirm";
    private readonly IUserNotificationUrl notificationUrl;
    private readonly IImageFormatter imageFormatter;
    private readonly IClock clock;
    private readonly ILogStore logstore;

    public UserNotificationFactory(ILogStore logstore, IUserNotificationUrl notificationUrl, IImageFormatter imageFormatter,
        IClock clock)
    {
        this.notificationUrl = notificationUrl;
        this.imageFormatter = imageFormatter;
        this.logstore = logstore;
        this.clock = clock;
    }

    public UserNotification? Create(App app, User user, UserEventMessage userEvent)
    {
        Guard.NotNull(user);
        Guard.NotNull(userEvent);

        if (userEvent.Formatting == null ||
            string.IsNullOrWhiteSpace(userEvent.AppId) ||
            string.IsNullOrWhiteSpace(userEvent.EventId) ||
            string.IsNullOrWhiteSpace(userEvent.Topic) ||
            string.IsNullOrWhiteSpace(userEvent.UserId))
        {
            return null;
        }

        var language = user.PreferredLanguage;

        if (!app.Languages.Contains(language))
        {
            logstore.LogAsync(app.Id, LogMessage.User_LanguageNotValid("System", user.Id, language, app.Language));
            language = app.Language;
        }

        var formatting = userEvent.Formatting.SelectText(language);

        if (!formatting.HasSubject())
        {
            return null;
        }

        var notification = SimpleMapper.Map(userEvent, new UserNotification
        {
            Id = Guid.NewGuid()
        });

        notification.Updated = clock.GetCurrentInstant();
        notification.UserLanguage = language;
        notification.Formatting = formatting;

        ConfigureImages(notification);
        ConfigureTracking(notification, userEvent);
        ConfigureSettings(notification, userEvent, user);

        return notification;
    }

    private void ConfigureImages(UserNotification notification)
    {
        notification.Formatting.ImageSmall = imageFormatter.AddProxy(notification.Formatting.ImageSmall);
        notification.Formatting.ImageLarge = imageFormatter.AddProxy(notification.Formatting.ImageLarge);
    }

    private void ConfigureTracking(UserNotification notification, UserEventMessage userEvent)
    {
        var confirmMode = userEvent.Formatting.ConfirmMode;

        if (confirmMode == ConfirmMode.Explicit)
        {
            notification.ConfirmUrl = notificationUrl.TrackConfirmed(notification.Id, notification.UserLanguage);

            if (string.IsNullOrWhiteSpace(notification.Formatting.ConfirmText))
            {
                notification.Formatting.ConfirmText = DefaultConfirmText;
            }
        }
        else
        {
            notification.Formatting.ConfirmText = null;
        }

        notification.TrackDeliveredUrl = notificationUrl.TrackDelivered(notification.Id, notification.UserLanguage);
        notification.TrackSeenUrl = notificationUrl.TrackSeen(notification.Id, notification.UserLanguage);
    }

    private static void ConfigureSettings(UserNotification notification, UserEventMessage userEvent, User user)
    {
        notification.Channels = new Dictionary<string, UserNotificationChannel>();

        OverrideBy(notification, user.Settings);
        OverrideBy(notification, userEvent.Settings);
    }

    private static void OverrideBy(UserNotification notification, ChannelSettings? source)
    {
        foreach (var (channel, setting) in source.OrEmpty())
        {
            notification.Channels.GetOrAddNew(channel).Setting.OverrideBy(setting);
        }
    }
}

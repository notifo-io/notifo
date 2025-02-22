﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Integrations;
using Notifo.Domain.UserNotifications;
using Notifo.Domain.Utils;

namespace Notifo.Domain.Channels.Email.Formatting;

public sealed class EmailNotification(BaseUserNotification notification, Guid configurationId, IImageFormatter imageFormatter)
{
    private string? confirmUrl;
    private string? imageLarge;
    private string? imageSmall;
    private string? trackDeliveredUrl;
    private string? trackSeenUrl;

    public string Subject => notification.Formatting.Subject;

    public string? Body => OrNull(notification.Formatting.Body);

    public string? LinkUrl => OrNull(notification.Formatting.LinkUrl);

    public string? LinkText => OrNull(notification.Formatting.LinkText);

    public string? ConfirmText => OrNull(notification.Formatting.ConfirmText);

    public string? TrackSeenUrl
    {
        get => trackSeenUrl ??= notification.ComputeTrackSeenUrl(Providers.Email, configurationId);
    }

    public string? TrackDeliveredUrl
    {
        get => trackDeliveredUrl ??= notification.ComputeTrackDeliveredUrl(Providers.Email, configurationId);
    }

    public string? ConfirmUrl
    {
        get => confirmUrl ??= notification.ComputeConfirmUrl(Providers.Email, configurationId);
    }

    public string? ImageSmall
    {
        get => imageSmall ??= notification.ImageSmall(imageFormatter, "EmailSmall");
    }

    public string? ImageLarge
    {
        get => imageLarge ??= notification.ImageLarge(imageFormatter, "EmailSmall");
    }

    private static string? OrNull(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value;
    }
}

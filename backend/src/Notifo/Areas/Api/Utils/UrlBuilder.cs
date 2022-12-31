// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Channels.Email;
using Notifo.Domain.Integrations;
using Notifo.Domain.UserNotifications;
using Squidex.Hosting;

namespace Notifo.Areas.Api.Utils;

public sealed class UrlBuilder : IEmailUrl, IMessagingUrl, ISmsUrl, IUserNotificationUrl
{
    private readonly IUrlGenerator urlGenerator;

    public UrlBuilder(IUrlGenerator urlGenerator)
    {
        this.urlGenerator = urlGenerator;
    }

    public string EmailPreferences(string apiKey, string language)
    {
        return urlGenerator.BuildUrl($"api/email-preferences?access_token={apiKey}&amp;culture={language}");
    }

    public string TrackConfirmed(Guid notificationId, string language)
    {
        return urlGenerator.BuildUrl($"api/tracking/notifications/{notificationId}/confirm?culture={language}");
    }

    public string TrackDelivered(Guid notificationId, string language)
    {
        return urlGenerator.BuildUrl($"api/tracking/notifications/{notificationId}/delivered?culture={language}");
    }

    public string TrackSeen(Guid notificationId, string language)
    {
        return urlGenerator.BuildUrl($"api/tracking/notifications/{notificationId}/seen?culture={language}");
    }

    public string SmsWebhookUrl(string appId, string integrationId)
    {
        return urlGenerator.BuildCallbackUrl($"api/callback/sms?appId={appId}&integrationId={integrationId}");
    }

    public string MessagingWebhookUrl(string appId, string integrationId)
    {
        return urlGenerator.BuildCallbackUrl($"api/callback/messaging?appId={appId}&integrationId={integrationId}");
    }
}

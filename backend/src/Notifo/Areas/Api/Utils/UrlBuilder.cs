// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Text;
using Notifo.Domain.Channels.Messaging;
using Notifo.Domain.Channels.Sms;
using Notifo.Domain.UserNotifications;
using Squidex.Hosting;

namespace Notifo.Areas.Api.Utils
{
    public sealed class UrlBuilder : IUserNotificationUrl, ISmsUrl, IMessagingUrl
    {
        private readonly IUrlGenerator urlGenerator;

        public UrlBuilder(IUrlGenerator urlGenerator)
        {
            this.urlGenerator = urlGenerator;
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

        public string SmsWebhookUrl(string appId, string integrationId, Dictionary<string, string>? query = null)
        {
            return urlGenerator.BuildCallbackUrl($"api/callback/sms?appId={appId}&integrationId={integrationId}{Query(query)}");
        }

        public string MessagingWebhookUrl(string appId, string integrationId, Dictionary<string, string>? query = null)
        {
            return urlGenerator.BuildCallbackUrl($"api/callback/messaging?appId={appId}&integrationId={integrationId}{Query(query)}");
        }

        private static string Query(Dictionary<string, string>? query = null)
        {
            if (query == null || query.Count == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder(10);

            foreach (var (key, value) in query)
            {
                sb.Append('&');
                sb.Append(key);
                sb.Append('=');
                sb.Append(Uri.EscapeDataString(value));
            }

            return sb.ToString();
        }
    }
}

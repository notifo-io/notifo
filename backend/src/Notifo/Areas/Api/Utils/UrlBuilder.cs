// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.Extensions.Options;
using Notifo.Domain.Channels.Sms;
using Notifo.Domain.UserNotifications;

namespace Notifo.Areas.Api.Utils
{
    public sealed class UrlBuilder : IUserNotificationUrl, ISmsUrl
    {
        private readonly UrlOptions urlOptions;

        public UrlBuilder(IOptions<UrlOptions> urlOptions)
        {
            this.urlOptions = urlOptions.Value;
        }

        public string TrackConfirmed(Guid notificationId, string language)
        {
            return urlOptions.BuildUrl($"api/tracking/notifications/{notificationId}/confirm?culture={language}");
        }

        public string TrackSeen(Guid notificationId, string language)
        {
            return urlOptions.BuildUrl($"api/tracking/notifications/{notificationId}/seen?culture={language}");
        }

        public string WebhookUrl()
        {
            return urlOptions.BuildCallbackUrl("api/callback/sms");
        }
    }
}

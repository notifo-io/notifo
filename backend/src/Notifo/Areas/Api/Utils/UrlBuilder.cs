// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Notifo.Domain.Channels.Sms;
using Notifo.Domain.UserNotifications;
using Squidex.Hosting;

namespace Notifo.Areas.Api.Utils
{
    public sealed class UrlBuilder : IUserNotificationUrl, ISmsUrl
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

        public string TrackSeen(Guid notificationId, string language)
        {
            return urlGenerator.BuildUrl($"api/tracking/notifications/{notificationId}/seen?culture={language}");
        }

        public string WebhookUrl()
        {
            return urlGenerator.BuildCallbackUrl("api/callback/sms");
        }
    }
}

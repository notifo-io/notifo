// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Areas.Api.Controllers.Notifications;
using Notifo.Areas.Api.Utils;
using Notifo.Domain.Channels.Messaging;
using Notifo.Domain.Channels.Sms;
using Notifo.Domain.Channels.Web;
using Notifo.Domain.UserNotifications;
using Notifo.Pipeline;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ApiServiceExtensions
    {
        public static void AddMyApi(this IServiceCollection services, SignalROptions signalROptions)
        {
            if (signalROptions.Enabled)
            {
                services.AddSingletonAs<NotificationHubAccessor>()
                    .As<IStreamClient>();
            }
            else
            {
                services.AddSingletonAs<NoopStreamClient>()
                    .As<IStreamClient>();
            }

            services.AddSingletonAs<UrlBuilder>()
                .As<IUserNotificationUrl>().As<ISmsUrl>().As<IMessagingUrl>();
        }
    }
}

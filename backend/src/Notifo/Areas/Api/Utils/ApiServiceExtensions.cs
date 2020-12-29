// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Areas.Api.Controllers.Notifications;
using Notifo.Areas.Api.Utils;
using Notifo.Domain.Channels.Sms;
using Notifo.Domain.Channels.Web;
using Notifo.Domain.UserNotifications;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ApiServiceExtensions
    {
        public static void AddMyApi(this IServiceCollection services)
        {
            services.AddSingletonAs<NotificationHubAccessor>()
                .As<IStreamClient>();

            services.AddSingletonAs<UrlBuilder>()
                .As<IUserNotificationUrl>().As<ISmsUrl>();
        }
    }
}

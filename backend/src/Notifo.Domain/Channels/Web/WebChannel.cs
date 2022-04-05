// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure;

namespace Notifo.Domain.Channels.Web
{
    public sealed class WebChannel : ICommunicationChannel
    {
        private readonly IStreamClient streamClient;
        private readonly IUserNotificationStore userNotificationStore;
        private readonly ILogger<WebChannel> log;

        public string Name => Providers.Web;

        public bool IsSystem => true;

        public WebChannel(IUserNotificationStore userNotificationStore, IStreamClient streamClient,
            ILogger<WebChannel> log)
        {
            this.streamClient = streamClient;
            this.userNotificationStore = userNotificationStore;

            this.log = log;
        }

        public IEnumerable<string> GetConfigurations(UserNotification notification, NotificationSetting settings, SendOptions options)
        {
            yield return Name;
        }

        public async Task SendAsync(UserNotification notification, NotificationSetting settings, string configuration, SendOptions options,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartActivity("WebChannel/SendAsync"))
            {
                try
                {
                    await streamClient.SendAsync(notification);

                    await userNotificationStore.CollectAndUpdateAsync(notification, Name, configuration, ProcessStatus.Handled, ct: ct);
                }
                catch (Exception ex)
                {
                    await userNotificationStore.CollectAndUpdateAsync(notification, Name, configuration, ProcessStatus.Failed, ct: ct);

                    log.LogError(ex, "Failed to send web message.");
                }
            }
        }
    }
}

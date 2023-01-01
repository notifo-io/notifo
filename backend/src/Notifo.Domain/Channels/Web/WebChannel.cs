// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using Notifo.Domain.Integrations;
using Notifo.Domain.UserNotifications;
using Notifo.Infrastructure;

namespace Notifo.Domain.Channels.Web;

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

    public IEnumerable<SendConfiguration> GetConfigurations(UserNotification notification, ChannelSetting settings, SendContext context)
    {
        yield return new SendConfiguration();
    }

    public async Task SendAsync(UserNotification notification, ChannelSetting settings, Guid configurationId, SendConfiguration properties, SendContext context,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("WebChannel/SendAsync"))
        {
            var identifier = TrackingKey.ForNotification(notification, Name, configurationId);
            try
            {
                await streamClient.SendAsync(notification);

                await userNotificationStore.TrackAsync(identifier, ProcessStatus.Handled, ct: default);
            }
            catch (Exception ex)
            {
                await userNotificationStore.TrackAsync(identifier, ProcessStatus.Failed, ct: ct);

                log.LogError(ex, "Failed to send web message.");
            }
        }
    }
}

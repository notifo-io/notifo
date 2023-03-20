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

public sealed class WebChannel : ChannelBase<WebChannel>
{
    private readonly IStreamClient streamClient;

    public override string Name => Providers.Web;

    public override bool IsSystem => true;

    public WebChannel(IServiceProvider serviceProvider,
        IStreamClient streamClient)
        : base(serviceProvider)
    {
        this.streamClient = streamClient;
    }

    public override IEnumerable<SendConfiguration> GetConfigurations(UserNotification notification, ChannelContext context)
    {
        yield return new SendConfiguration();
    }

    public override async Task SendAsync(UserNotification notification, ChannelContext context,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("WebChannel/SendAsync"))
        {
            var identifier = TrackingKey.ForNotification(notification, Name, context.ConfigurationId);
            try
            {
                await streamClient.SendAsync(notification);

                await UserNotificationStore.TrackAsync(identifier, DeliveryResult.Sent, default);
            }
            catch (Exception ex)
            {
                await UserNotificationStore.TrackAsync(identifier, DeliveryResult.Failed(), ct: ct);

                Log.LogError(ex, "Failed to send web message.");
            }
        }
    }
}

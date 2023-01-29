// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Domain;
using Notifo.Domain.Channels;
using Notifo.Domain.Integrations;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.Notifications.Dtos;

public sealed class ChannelSendInfoDto
{
    /// <summary>
    /// The send status.
    /// </summary>
    public DeliveryStatus Status { get; set; }

    /// <summary>
    /// The configuration for the device.
    /// </summary>
    public SendConfiguration Configuration { get; init; }

    /// <summary>
    /// The last update.
    /// </summary>
    public Instant LastUpdate { get; set; }

    /// <summary>
    /// The details.
    /// </summary>
    public string? Detail { get; set; }

    /// <summary>
    /// The first time the notification has been marked as delivered for this identifier.
    /// </summary>
    public Instant? FirstDelivered { get; set; }

    /// <summary>
    /// The first time the notification has been marked as seen for this identifier.
    /// </summary>
    public Instant? FirstSeen { get; set; }

    /// <summary>
    /// The first time the notification has been marked as confirmed for this identifier.
    /// </summary>
    public Instant? FirstConfirmed { get; set; }

    public static ChannelSendInfoDto FromDomainObject(ChannelSendInfo source)
    {
        return SimpleMapper.Map(source, new ChannelSendInfoDto());
    }
}

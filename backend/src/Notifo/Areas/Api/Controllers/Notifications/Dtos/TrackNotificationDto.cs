// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Areas.Api.Controllers.Notifications.Dtos;

public sealed class TrackNotificationDto
{
    /// <summary>
    /// The id of the noitifications to mark as confirmed.
    /// </summary>
    public string? Confirmed { get; set; }

    /// <summary>
    /// The id of the noitifications to mark as seen.
    /// </summary>
    public string[]? Seen { get; set; }

    /// <summary>
    /// The channel name.
    /// </summary>
    public string? Channel { get; set; }

    /// <summary>
    /// The configuration ID.
    /// </summary>
    public Guid ConfigurationId { get; set; }

    /// <summary>
    /// The device identifier.
    /// </summary>
    public string? DeviceIdentifier { get; set; }
}

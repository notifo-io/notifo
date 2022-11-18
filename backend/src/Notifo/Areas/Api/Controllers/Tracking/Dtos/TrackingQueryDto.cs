// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Areas.Api.Controllers.Tracking.Dtos;

public class TrackingQueryDto
{
    public string? Channel { get; set; }

    public Guid ConfigurationId { get; set; }

    public string? DeviceIdentifier { get; set; }
}

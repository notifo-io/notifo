// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Areas.Api.OpenApi;

namespace Notifo.Areas.Api.Controllers.Tracking.Dtos;

[OpenApiRequest]
public class TrackingQueryDto
{
    public string? Channel { get; set; }

    public Guid ConfigurationId { get; set; }

    public string? DeviceIdentifier { get; set; }
}

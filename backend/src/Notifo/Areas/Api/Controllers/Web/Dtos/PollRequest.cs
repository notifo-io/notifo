// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;

namespace Notifo.Areas.Api.Controllers.Web.Dtos;

public class PollRequest
{
    public Guid[]? Deleted { get; set; }

    public string[]? Delivered { get; set; }

    public string[]? Seen { get; set; }

    public string[]? Confirmed { get; set; }

    public Instant? Token { get; set; }
}

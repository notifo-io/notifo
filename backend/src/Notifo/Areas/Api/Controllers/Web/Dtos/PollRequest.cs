// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;

namespace Notifo.Areas.Api.Controllers.Web.Dtos
{
    public class PollRequest
    {
        public Guid[]? Deleted { get; set; }

        public Guid[]? Delivered { get; set; }

        public Guid[]? Seen { get; set; }

        public Guid[]? Confirmed { get; set; }

        public Instant? Token { get; set; }
    }
}

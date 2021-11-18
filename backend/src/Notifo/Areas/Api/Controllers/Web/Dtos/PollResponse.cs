// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Areas.Api.Controllers.Notifications.Dtos;

namespace Notifo.Areas.Api.Controllers.Web.Dtos
{
    public sealed class PollResponse
    {
        public Instant ContinuationToken { get; set; }

        public List<UserNotificationDto> Notifications { get; } = new List<UserNotificationDto>();

        public List<Guid>? Deletions { get; set; }
    }
}

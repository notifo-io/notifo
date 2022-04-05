// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Notifo.Domain.UserNotifications
{
    public sealed record HandledInfo(Instant Timestamp, string? Channel);
}

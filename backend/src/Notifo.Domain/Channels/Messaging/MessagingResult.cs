// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Channels.Messaging
{
    public enum MessagingResult
    {
        Unknown,
        Skipped,
        Sent,
        Delivered,
        Failed
    }
}

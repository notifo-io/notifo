// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Integrations.MessageBird.Implementation
{
    public enum MessageBirdStatus
    {
        Answered,
        Buffered,
        Calling,
        Delivered,
        Delivery_Failed,
        Expired,
        Failed,
        Scheduled,
        Sent
    }
}

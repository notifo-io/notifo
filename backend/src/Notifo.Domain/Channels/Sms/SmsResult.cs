// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Channels.Sms
{
    public enum SmsResult
    {
        Unknown,
        Skipped,
        Sent,
        Delivered,
        Failed
    }
}

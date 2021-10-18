// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Channels.Sms
{
    public struct SmsResponse
    {
        public string ReferenceNumber { get; init; }

        public string Reference { get; init; }

        public SmsResult Status { get; init; }
    }
}

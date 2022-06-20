// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Integrations.MessageBird.Implementation
{
    public sealed class SmsStatus
    {
        public string Id { get; set; }

        public string Recipient { get; set; }

        public int StatusErrorCode { get; set; }

        public Guid Reference { get; set; }

        public MessageBirdStatus Status { get; set; }
    }
}

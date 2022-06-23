// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Integrations.MessageBird.Implementation
{
    public sealed class SmsWebhookRequest
    {
        public string Id { get; set; }

        public string Recipient { get; set; }

        public string Reference { get; set; }

        public int StatusErrorCode { get; set; }

        public MessageBirdStatus Status { get; set; }
    }
}

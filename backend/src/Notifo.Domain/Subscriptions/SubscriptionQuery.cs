// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure;

namespace Notifo.Domain.Subscriptions
{
    public sealed class SubscriptionQuery : QueryBase
    {
        public string? UserId { get; set; }

        public string? Query { get; set; }

        public string[]? Topics { get; set; }
    }
}

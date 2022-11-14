// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Subscriptions;

public abstract class SubscriptionCommand : AppCommandBase<Subscription>
{
    public string UserId { get; set; }

    public TopicId Topic { get; set; }
}

// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Subscriptions;

public abstract class SubscriptionCommand : UserCommandBase<Subscription>
{
    public TopicId Topic { get; set; }
}

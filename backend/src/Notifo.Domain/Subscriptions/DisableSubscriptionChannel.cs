// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure;

namespace Notifo.Domain.Subscriptions
{
    public sealed class DisableSubscriptionChannel : ICommand<Subscription>
    {
        public string Channel { get; set; }

        public ValueTask<Subscription?> ExecuteAsync(Subscription target, IServiceProvider serviceProvider,
            CancellationToken ct)
        {
            if (target.TopicSettings?.TryGetValue(Channel, out var setting) != true || setting?.Send != ChannelSend.Send)
            {
                return new ValueTask<Subscription?>(target);
            }

            var newSetting = setting with
            {
                Send = ChannelSend.NotSending
            };

            var newSettings = new ChannelSettings(target.TopicSettings ?? new ChannelSettings())
            {
                [Channel] = newSetting
            };

            var newSubscription = target with
            {
                TopicSettings = newSettings
            };

            return new ValueTask<Subscription?>(newSubscription);
        }
    }
}

// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Notifo.Domain.Users;
using Notifo.Infrastructure;

namespace Notifo.Domain.Subscriptions
{
    public sealed class Subscribe : ICommand<Subscription>
    {
        public NotificationSettings? TopicSettings { get; set; }

        public bool CanCreate => true;

        public async ValueTask<Subscription?> ExecuteAsync(Subscription subscription, IServiceProvider serviceProvider,
            CancellationToken ct)
        {
            var userStore = serviceProvider.GetRequiredService<IUserStore>();

            await CheckWhitelistAsync(userStore, subscription, ct);

            var newSubscription = subscription with
            {
                TopicSettings = TopicSettings ?? subscription.TopicSettings
            };

            return newSubscription;
        }

        private static async Task CheckWhitelistAsync(IUserStore userStore, Subscription subscription,
            CancellationToken ct)
        {
            var user = await userStore.GetCachedAsync(subscription.AppId, subscription.UserId, ct);

            if (user == null)
            {
                throw new DomainObjectNotFoundException(subscription.UserId);
            }

            if (user.AllowedTopics == null)
            {
                return;
            }

            if (user.AllowedTopics.Count == 0 && !user.RequiresWhitelistedTopics)
            {
                return;
            }

            if (!user.AllowedTopics.Any(x => subscription.TopicPrefix.StartsWith(x)))
            {
                throw new DomainForbiddenException("Topic is not whitelisted.");
            }
        }
    }
}

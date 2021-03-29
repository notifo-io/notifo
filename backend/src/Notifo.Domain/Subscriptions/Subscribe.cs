// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Notifo.Domain.Users;
using Notifo.Infrastructure;

namespace Notifo.Domain.Subscriptions
{
    public sealed class Subscribe : ICommand<Subscription>
    {
        public NotificationSettings? TopicSettings { get; set; }

        public bool CanCreate => true;

        public async Task<bool> ExecuteAsync(Subscription target, IServiceProvider serviceProvider, CancellationToken ct)
        {
            var userStore = serviceProvider.GetRequiredService<IUserStore>();

            await CheckWhitelistAsync(userStore, target, ct);

            if (TopicSettings != null)
            {
                target.TopicSettings ??= new NotificationSettings();

                foreach (var (key, value) in TopicSettings)
                {
                    target.TopicSettings[key] = value;
                }
            }

            return true;
        }

        private static async Task CheckWhitelistAsync(IUserStore userStore, Subscription target, CancellationToken ct)
        {
            var user = await userStore.GetCachedAsync(target.AppId, target.UserId, ct);

            if (user == null)
            {
                throw new DomainObjectNotFoundException(target.UserId);
            }

            if (user.AllowedTopics == null)
            {
                return;
            }

            if (user.AllowedTopics.Count == 0 && !user.RequiresWhitelistedTopics)
            {
                return;
            }

            if (!user.AllowedTopics.Any(x => target.TopicPrefix.StartsWith(x)))
            {
                throw new DomainForbiddenException("Topic is not whitelisted.");
            }
        }
    }
}

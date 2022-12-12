// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Notifo.Domain.Users;
using Notifo.Infrastructure;

namespace Notifo.Domain.Subscriptions;

public sealed class Subscribe : SubscriptionCommand
{
    public ChannelSettings? TopicSettings { get; set; }

    public Scheduling? Scheduling { get; set; }

    public bool HasScheduling { get; set; }

    public bool MergeSettings { get; set; }

    public override bool CanCreate => true;

    public override async ValueTask<Subscription?> ExecuteAsync(Subscription target, IServiceProvider serviceProvider,
        CancellationToken ct)
    {
        var userStore = serviceProvider.GetRequiredService<IUserStore>();

        await CheckWhitelistAsync(userStore, target, ct);

        var newSettings = TopicSettings;

        if (MergeSettings || newSettings == null)
        {
            newSettings = ChannelSettings.Merged(target.TopicSettings, TopicSettings);
        }

        var newSubscription = target with
        {
            TopicSettings = newSettings
        };

        if (HasScheduling)
        {
            newSubscription = newSubscription with
            {
                Scheduling = Scheduling
            };
        }

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

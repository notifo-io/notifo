// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure;
using Notifo.Infrastructure.Mediator;

namespace Notifo.Domain.Subscriptions;

public sealed class SubscriptionStore : ISubscriptionStore, IRequestHandler<SubscriptionCommand, Subscription?>
{
    private readonly ISubscriptionRepository repository;
    private readonly IServiceProvider serviceProvider;

    public SubscriptionStore(ISubscriptionRepository repository,
        IServiceProvider serviceProvider)
    {
        this.repository = repository;
        this.serviceProvider = serviceProvider;
    }

    public IAsyncEnumerable<Subscription> QueryAsync(string appId, TopicId topic, string? userId,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(appId);

        return repository.QueryAsync(appId, topic, userId, ct);
    }

    public Task<IResultList<Subscription>> QueryAsync(string appId, SubscriptionQuery query,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(appId);

        return repository.QueryAsync(appId, query, ct);
    }

    public async Task<Subscription?> GetAsync(string appId, string userId, TopicId prefix,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(appId);

        var (subscription, _) = await repository.GetAsync(appId, userId, prefix, ct);

        return subscription;
    }

    public async ValueTask<Subscription?> HandleAsync(SubscriptionCommand command,
        CancellationToken ct)
    {
        Guard.NotNullOrEmpty(command.AppId);
        Guard.NotNullOrEmpty(command.UserId);

        if (!command.IsUpsert)
        {
            await command.ExecuteAsync(serviceProvider, ct);
            return null;
        }

        return await Updater.UpdateRetriedAsync(5, async () =>
        {
            var (subscription, etag) = await repository.GetAsync(command.AppId, command.UserId, command.Topic, ct);

            if (subscription == null)
            {
                if (!command.CanCreate)
                {
                    throw new DomainObjectNotFoundException(command.Topic.ToString());
                }

                subscription = Subscription.Create(command.AppId, command.UserId, command.Topic);
            }

            var newSubscription = await command.ExecuteAsync(subscription, serviceProvider, ct);

            if (newSubscription != null && !ReferenceEquals(subscription, newSubscription))
            {
                await repository.UpsertAsync(newSubscription, etag, ct);
                newSubscription = subscription;
            }

            return subscription;
        });
    }
}

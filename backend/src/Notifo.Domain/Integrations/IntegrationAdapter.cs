// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Users;
using Notifo.Infrastructure.Mediator;

namespace Notifo.Domain.Integrations;

internal class IntegrationAdapter : IIntegrationAdapter
{
    private readonly IUserStore userStore;
    private readonly IMediator mediator;

    public IntegrationAdapter(IUserStore userStore, IMediator mediator)
    {
        this.userStore = userStore;
        this.mediator = mediator;
    }

    public async Task<UserContext?> FindUserAsync(string appId, string id,
        CancellationToken ct)
    {
        var user = await userStore.GetAsync(appId, id, ct);

        return user?.ToContext();
    }

    public async Task<UserContext?> FindUserByPropertyAsync(string appId, string key, string value,
        CancellationToken ct)
    {
        var user = await userStore.GetByPropertyAsync(appId, key, value, ct);

        return user?.ToContext();
    }

    public async Task UpdateUserAsync(string appId, string id, string key, string value,
        CancellationToken ct)
    {
        var command = new SetUserSystemProperty
        {
            PropertyKey = key,
            PropertyValue = value
        }.WithTracking(appId, id);

        await mediator.SendAsync(command, ct);
    }
}

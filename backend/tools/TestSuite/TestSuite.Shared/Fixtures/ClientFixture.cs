// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.SDK;
using Xunit;

namespace TestSuite.Fixtures;

public class ClientFixture : IAsyncLifetime
{
    public ClientManagerWrapper Notifo { get; private set; }

    public string AppName => Notifo.AppName;

    public string ClientId => Notifo.ClientId;

    public string ClientSecret => Notifo.ClientSecret;

    public string ServerUrl => Notifo.ServerUrl;

    public INotifoClient Client => Notifo.Client;

    public virtual async Task InitializeAsync()
    {
        Notifo = await Factories.CreateAsync(nameof(ClientManagerWrapper), async () =>
        {
            var clientManager = new ClientManagerWrapper();

            await clientManager.ConnectAsync();

            return clientManager;
        });
    }

    public INotifoClient BuildUserClient(UserDto user)
    {
        return NotifoClientBuilder.Create()
            .SetApiUrl(ServerUrl)
            .SetApiKey(user.ApiKey)
            .Build();
    }

    public virtual Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}

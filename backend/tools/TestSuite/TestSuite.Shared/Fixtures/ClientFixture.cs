// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Concurrent;
using Notifo.SDK;
using Xunit;

namespace TestSuite.Fixtures;

public class ClientFixture : IAsyncLifetime
{
    private readonly ConcurrentDictionary<string, INotifoClient> clients = new ConcurrentDictionary<string, INotifoClient>();

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
        return BuildClient(user.ApiKey);
    }

    public INotifoClient BuildAppClient(AppDto app)
    {
        return BuildClient(app.ApiKeys.First(x => x.Value != "WebManager").Key);
    }

    public INotifoClient BuildClient(string apiKey)
    {
        return clients.GetOrAdd(apiKey, (x, self) =>
        {
            return NotifoClientBuilder.Create()
                .SetApiUrl(self.ServerUrl)
                .SetApiKey(x)
                .Build();
        }, this);
    }

    public virtual Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}

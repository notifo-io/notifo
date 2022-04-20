// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.SDK;
using Xunit;

namespace TestSuite.Fixtures
{
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
            Notifo = await ClientManagerWrapper.CreateAsync();
        }

        public virtual Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}

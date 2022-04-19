// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.SDK;

namespace TestSuite.Fixtures
{
    public class ClientFixture : IDisposable
    {
        public ClientManagerWrapper Notifo { get; }

        public string AppName => Notifo.AppName;

        public string ClientId => Notifo.ClientId;

        public string ClientSecret => Notifo.ClientSecret;

        public string ServerUrl => Notifo.ServerUrl;

        public INotifoClient Client => Notifo.Client;

        public ClientFixture()
        {
            Notifo = ClientManagerWrapper.CreateAsync().Result;
        }

        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}

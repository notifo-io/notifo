// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace TestSuite
{
    public static class ClientManagerFactory
    {
        private static readonly Task<ClientManagerWrapper> Instance = CreateInternalAsync();

        public static Task<ClientManagerWrapper> CreateAsync()
        {
            return Instance;
        }

        private static async Task<ClientManagerWrapper> CreateInternalAsync()
        {
            var clientManager = new ClientManagerWrapper();

            await clientManager.ConnectAsync();

            return clientManager;
        }
    }
}

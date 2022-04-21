// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Notifo.SDK;
using TestSuite.Utils;

namespace TestSuite
{
    public sealed class ClientManagerWrapper
    {
        public INotifoClient Client { get; set; }

        public string AppName { get; }

        public string ClientId { get; }

        public string ClientSecret { get; }

        public string ServerUrl { get; }

        public ClientManagerWrapper()
        {
            AppName = TestHelpers.GetAndPrintValue("config:app:name", "integration-tests");

            ClientId = TestHelpers.GetAndPrintValue("config:client:id", "root");
            ClientSecret = TestHelpers.GetAndPrintValue("config:client:secret", "xeLd6jFxqbXJrfmNLlO2j1apagGGGSyZJhFnIuHp4I0=");

            ServerUrl = TestHelpers.GetAndPrintValue("config:server:url", "https://localhost:5002");

            Client =
                NotifoClientBuilder.Create()
                    .ReadResponseAsString(true)
                    .SetClientId(ClientId)
                    .SetClientSecret(ClientSecret)
                    .SetApiUrl(ServerUrl)
                    .SetTimeout(Timeout())
                    .Build();
        }

        public INotifoClient CreateUserClient(UserDto user)
        {
            var userClient =
                NotifoClientBuilder.Create()
                    .ReadResponseAsString(true)
                    .SetApiKey(user.ApiKey)
                    .SetApiUrl(ServerUrl)
                    .SetTimeout(Timeout())
                    .Build();

            return userClient;
        }

        private static TimeSpan Timeout()
        {
            return Debugger.IsAttached ?
                TimeSpan.FromMinutes(5) :
                TimeSpan.FromSeconds(10);
        }

        public async Task ConnectAsync()
        {
            var waitSeconds = TestHelpers.Configuration.GetValue<int>("config:wait");

            if (waitSeconds > 10)
            {
                Console.WriteLine("Waiting {0} seconds to access server", waitSeconds);

                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(waitSeconds)))
                {
                    while (!cts.IsCancellationRequested)
                    {
                        try
                        {
                            await Client.Ping.GetPingAsync(cts.Token);

                            break;
                        }
                        catch
                        {
                            await Task.Delay(100);
                        }
                    }
                }

                Console.WriteLine("Connected to server");
            }
            else
            {
                Console.WriteLine("Waiting for server is skipped.");
            }
        }
    }
}

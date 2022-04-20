﻿// ==========================================================================
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
        private static readonly Task<ClientManagerWrapper> Instance = CreateInternalAsync();

        public INotifoClient Client { get; set; }

        public string AppName { get; }

        public string ClientId { get; }

        public string ClientSecret { get; }

        public string ServerUrl { get; }

        public ClientManagerWrapper()
        {
            AppName = GetValue("config:app:name", "integration-tests");

            ClientId = GetValue("config:client:id", "root");
            ClientSecret = GetValue("config:client:secret", "xeLd6jFxqbXJrfmNLlO2j1apagGGGSyZJhFnIuHp4I0=");

            ServerUrl = GetValue("config:server:url", "https://localhost:5002");

            var timeout =
                Debugger.IsAttached ?
                    TimeSpan.FromMinutes(5) :
                    TimeSpan.FromSeconds(10);

            Client =
                NotifoClientBuilder.Create()
                    .ReadResponseAsString(true)
                    .SetClientId(ClientId)
                    .SetClientSecret(ClientSecret)
                    .SetApiUrl(ServerUrl)
                    .SetTimeout(timeout)
                    .Build();
        }

        public static Task<ClientManagerWrapper> CreateAsync()
        {
            return Instance;
        }

        private static Task<ClientManagerWrapper> CreateInternalAsync()
        {
            var clientManager = new ClientManagerWrapper();

            return clientManager.ConnectAsync();
        }

        public async Task<ClientManagerWrapper> ConnectAsync()
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

            return this;
        }

        private static string GetValue(string name, string fallback)
        {
            var value = TestHelpers.Configuration[name];

            if (string.IsNullOrWhiteSpace(value))
            {
                value = fallback;
            }
            else
            {
                Console.WriteLine("Using: {0}={1}", name, value);
            }

            return value;
        }
    }
}

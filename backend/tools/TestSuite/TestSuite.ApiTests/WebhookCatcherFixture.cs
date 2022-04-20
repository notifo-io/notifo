// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using TestSuite.Utils;

namespace TestSuite.ApiTests
{
    public sealed class WebhookCatcherFixture
    {
        internal WebhookCatcherClient Client { get; }

        public WebhookCatcherFixture()
        {
            var apiHost = TestHelpers.Configuration["webhookcatcher:host:api"];
            var apiPort = 1026;

            if (string.IsNullOrWhiteSpace(apiHost))
            {
                apiHost = "localhost";
            }

            var endpointHost = TestHelpers.Configuration["webhookcatcher:host:endpoint"];
            var endpointPort = 1026;

            if (string.IsNullOrWhiteSpace(endpointHost))
            {
                endpointHost = "localhost";
            }

            Console.WriteLine("Using Webhookcatcher with Host {0}:{1} and Smtp Host {2}:{3}", apiHost, apiPort, endpointHost, endpointPort);

            Client = new WebhookCatcherClient(apiHost, apiPort, endpointHost, endpointPort);
        }
    }
}

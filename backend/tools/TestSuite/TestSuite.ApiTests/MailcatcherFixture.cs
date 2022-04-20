// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using TestSuite.Utils;

namespace TestSuite.ApiTests
{
    public sealed class MailcatcherFixture
    {
        internal MailcatcherClient Client { get; }

        public MailcatcherFixture()
        {
            var apiHost = TestHelpers.Configuration["mailcatcher:host:api"];
            var apiPort = 1080;

            if (string.IsNullOrWhiteSpace(apiHost))
            {
                apiHost = "localhost";
            }

            var smtpHost = TestHelpers.Configuration["mailcatcher:host:smtp"];
            var smtpPort = 1025;

            if (string.IsNullOrWhiteSpace(smtpHost))
            {
                smtpHost = "localhost";
            }

            Console.WriteLine("Using Mailcatcher with Host {0}:{1} and Smtp Host {2}:{3}", apiHost, apiPort, smtpHost, smtpPort);

            Client = new MailcatcherClient(apiHost, apiPort, smtpHost, smtpPort);
        }
    }
}

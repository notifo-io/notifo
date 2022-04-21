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
            Client = new MailcatcherClient(
                TestHelpers.GetAndPrintValue("mailcatcher:host:api", "localhost"), 1080,
                TestHelpers.GetAndPrintValue("mailcatcher:host:smtp", "localhost"), 1025);
        }
    }
}

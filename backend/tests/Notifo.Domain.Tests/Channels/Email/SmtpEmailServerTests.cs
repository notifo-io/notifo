// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Xunit;

namespace Notifo.Domain.Channels.Email
{
    [Trait("Category", "Dependencies")]
    public class SmtpEmailServerTests : EmailServerTestBase
    {
        protected override IEmailServer CreateServer()
        {
            return new SmtpEmailServer(Options.Create(new SmtpOptions
            {
                Host = TestHelpers.Configuration.GetValue<string>("email:smtp:host"),
                Password = TestHelpers.Configuration.GetValue<string>("email:smtp:password"),
                Username = TestHelpers.Configuration.GetValue<string>("email:smtp:username")
            }));
        }
    }
}

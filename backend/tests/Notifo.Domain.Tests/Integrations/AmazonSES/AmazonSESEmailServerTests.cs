// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Notifo.Domain.Integrations.AmazonSES;
using Notifo.Domain.Integrations.Smtp;
using Xunit;

namespace Notifo.Domain.Channels.Email
{
    [Trait("Category", "Dependencies")]
    public class AmazonSESEmailServerTests : EmailServerTestBase
    {
        protected override IEmailSender CreateServer()
        {
            return new SmtpEmailServer(new AmazonSESOptions
            {
                Host = TestHelpers.Configuration.GetValue<string>("email:amazonSES:host"),
                Password = TestHelpers.Configuration.GetValue<string>("email:amazonSES:password"),
                Username = TestHelpers.Configuration.GetValue<string>("email:amazonSES:username")
            });
        }
    }
}

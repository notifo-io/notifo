// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Notifo.Domain.Integrations;
using Notifo.Domain.Integrations.AmazonSES;
using Notifo.Domain.Integrations.Smtp;
using Xunit;

namespace Notifo.Domain.Channels.Email
{
    [Trait("Category", "Dependencies")]
    public class AmazonSESEmailServerTests : EmailSenderTestBase
    {
        protected override IEmailSender CreateSender()
        {
            var options = TestHelpers.Configuration.GetSection("email:amazonSES").Get<AmazonSESOptions>();

            return new SmtpEmailSender(new SmtpEmailServer(options), Address, Address);
        }
    }
}

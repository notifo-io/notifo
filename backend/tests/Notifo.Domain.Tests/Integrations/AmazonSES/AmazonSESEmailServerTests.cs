// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Notifo.Domain.Integrations.Smtp;

namespace Notifo.Domain.Integrations.AmazonSES;

[Trait("Category", "Dependencies")]
public class AmazonSESEmailServerTests : EmailSenderTestBase
{
    protected override IEmailSender CreateSender()
    {
        var options = TestHelpers.Configuration.GetSection("email:amazonSES").Get<AmazonSESOptions>() ?? new AmazonSESOptions();

        return new SmtpEmailSender(() => new SmtpEmailServer(options), Address, Address);
    }
}

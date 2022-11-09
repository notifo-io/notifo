// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Notifo.Domain.Channels.Email;
using Xunit;

namespace Notifo.Domain.Integrations.Smtp;

[Trait("Category", "Dependencies")]
public class SmtpEmailServerTests : EmailSenderTestBase
{
    protected override IEmailSender CreateSender()
    {
        var options = TestHelpers.Configuration.GetSection("email:smtp").Get<SmtpOptions>();

        return new SmtpEmailSender(() => new SmtpEmailServer(options), Address, Address);
    }
}

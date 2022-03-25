// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Mjml.Net;
using Notifo.Domain.Channels.Email.Formatting;

#pragma warning disable MA0056 // Do not call overridable members in constructor

namespace Notifo.Domain.Channels.Email
{
    public class EmailTemplateLiquidTests : EmailTemplateTestsBase
    {
        protected override EmailTemplate EmailTemplate { get; }

        protected override IEmailFormatter EmailFormatter { get; } = new EmailFormatterLiquid(new FakeImageFormatter(), new MjmlRenderer());

        public EmailTemplateLiquidTests()
        {
            EmailTemplate = EmailFormatter.CreateInitialAsync("Liquid").AsTask().Result;
        }
    }
}

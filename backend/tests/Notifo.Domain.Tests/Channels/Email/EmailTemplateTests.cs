// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Mjml.Net;
using Notifo.Domain.Channels.Email.Formatting;
using Notifo.Domain.Utils;

namespace Notifo.Domain.Channels.Email;

public class EmailTemplateTests : EmailTemplateTestsBase
{
    protected override string Name => "normal";

    protected override IEmailFormatter CreateFormatter(IEmailUrl url, IImageFormatter imageFormatter)
    {
        return new EmailFormatterNormal(imageFormatter, url, new MjmlRenderer());
    }
}

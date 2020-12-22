// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Options;

namespace Notifo.Domain.Channels.Email
{
    public sealed class SmtpEmailServer : SmtpEmailServerBase
    {
        public SmtpEmailServer(IOptions<SmtpOptions> options)
            : base(options.Value)
        {
        }
    }
}

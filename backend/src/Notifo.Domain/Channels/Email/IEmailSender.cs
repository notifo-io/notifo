// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;

namespace Notifo.Domain.Channels.Email
{
    public interface IEmailSender
    {
        Task SendAsync(EmailMessage message,
            CancellationToken ct = default);
    }
}

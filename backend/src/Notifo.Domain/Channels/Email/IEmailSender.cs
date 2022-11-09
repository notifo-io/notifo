// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Channels.Email;

public interface IEmailSender
{
    string Name { get; }

    Task SendAsync(EmailMessage message,
        CancellationToken ct = default);
}

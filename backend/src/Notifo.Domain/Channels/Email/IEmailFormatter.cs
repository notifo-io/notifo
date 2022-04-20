// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Apps;
using Notifo.Domain.ChannelTemplates;
using Notifo.Domain.Users;

namespace Notifo.Domain.Channels.Email
{
    public interface IEmailFormatter : IChannelTemplateFactory<EmailTemplate>
    {
        bool Accepts(string? kind);

        ValueTask<FormattedEmail> FormatAsync(List<EmailJob> jobs, EmailTemplate template, App app, User user, bool noCache = false,
            CancellationToken ct = default);
    }
}

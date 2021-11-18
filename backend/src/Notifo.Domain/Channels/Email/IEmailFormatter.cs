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
        ValueTask<EmailMessage> FormatPreviewAsync(List<EmailJob> jobs, EmailTemplate template, App app, User user,
            CancellationToken ct = default);

        ValueTask<EmailMessage> FormatAsync(List<EmailJob> jobs, EmailTemplate template, App app, User user,
            CancellationToken ct = default);
    }
}

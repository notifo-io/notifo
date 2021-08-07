// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Notifo.Domain.Apps;
using Notifo.Domain.ChannelTemplates;
using Notifo.Domain.UserNotifications;
using Notifo.Domain.Users;

namespace Notifo.Domain.Channels.Email
{
    public interface IEmailFormatter : IChannelTemplateFactory<EmailTemplate>
    {
        ValueTask<EmailMessage> FormatPreviewAsync(IEnumerable<BaseUserNotification> notifications, EmailTemplate template, App app, User user,
            CancellationToken ct = default);

        ValueTask<EmailMessage> FormatAsync(IEnumerable<BaseUserNotification> notifications, EmailTemplate template, App app, User user,
            CancellationToken ct = default);
    }
}

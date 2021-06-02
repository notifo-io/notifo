// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Notifo.Domain.Apps;
using Notifo.Domain.UserNotifications;
using Notifo.Domain.Users;

namespace Notifo.Domain.Channels.Email
{
    public interface IEmailFormatter
    {
        ValueTask<EmailTemplate> GetDefaultTemplateAsync();

        ValueTask<EmailTemplate> ParseAsync(EmailTemplate template);

        ValueTask<EmailMessage> FormatPreviewAsync(IEnumerable<BaseUserNotification> notifications, EmailTemplate template, App app, User user);

        ValueTask<EmailMessage> FormatAsync(IEnumerable<BaseUserNotification> notifications, App app, User user);
    }
}

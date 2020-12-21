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
        Task<EmailTemplate> GetDefaultTemplateAsync();

        Task<EmailMessage> FormatAsync(IEnumerable<UserNotification> notifications, EmailTemplate template, App app, User user, bool noCache);

        Task<EmailMessage> FormatAsync(IEnumerable<UserNotification> notifications, App app, User user);
    }
}

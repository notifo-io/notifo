// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Fluid;
using Notifo.Domain.Apps;
using Notifo.Domain.Resources;
using Notifo.Domain.Users;
using Notifo.Domain.Utils;

#pragma warning disable RECS0082 // Parameter has the same name as a member and hides it
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Notifo.Domain.Channels.Email.Formatting
{
    public sealed partial class EmailFormatterLiquid
    {
        private sealed record Context(TemplateContext TemplateContext, IReadOnlyList<EmailJob> Jobs, App App, User User)
        {
            public List<EmailFormattingError>? Errors { get; private set; }

            public void AddError(string message, EmailTemplateType template, int? line = -1, int? column = -1)
            {
                Errors ??= new List<EmailFormattingError>();
                Errors.Add(new EmailFormattingError(message, template, line ?? -1, column ?? -1));
            }

            public static Context Create(IReadOnlyList<EmailJob> jobs, App app, User user,
                IImageFormatter imageFormatter, IEmailUrl emailUrl)
            {
                var templateContext = new TemplateContext(Options);

                var emailNotifications = jobs.Select(x => new EmailNotification(x.Notification, x.EmailAddress, imageFormatter)).ToArray();
                var emailPreferences = emailUrl.EmailPreferences(user.ApiKey, user.PreferredLanguage);

                templateContext.SetValue("app", app);
                templateContext.SetValue("user", user);
                templateContext.SetValue("notifications", emailNotifications);
                templateContext.SetValue("preferencesUrl", emailPreferences);

                return new Context(templateContext, jobs, app, user);
            }

            public void ValidateTemplate(string? template, EmailTemplateType type)
            {
                if (string.IsNullOrWhiteSpace(template))
                {
                    return;
                }

                if (Jobs.Any(x => !template.Contains(x.Notification.Formatting.Subject, StringComparison.OrdinalIgnoreCase)))
                {
                    AddError(Texts.Email_TemplateLiquidInvalid, type);
                }
            }
        }
    }
}

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

namespace Notifo.Domain.Channels.Email.Formatting;

public sealed partial class EmailFormatterLiquid
{
    private sealed class Context
    {
        public App App { get; private init; }

        public List<EmailFormattingError> Errors { get; } = new List<EmailFormattingError>();

        public List<EmailJob> Jobs { get; private init; }

        public TemplateContext TemplateContext { get; private init; }

        public User User { get; private init; }

        public string? EmailAddress => User.EmailAddress;

        private Context()
        {
        }

        public static Context Create(IReadOnlyList<EmailJob> jobs, App app, User user, IImageFormatter imageFormatter, IEmailUrl emailUrl)
        {
            var templateContext = new TemplateContext(Options);

            var emailNotifications = jobs.Select(x => new EmailNotification(x.Notification, x.ConfigurationId, x.EmailAddress, imageFormatter)).ToArray();
            var emailPreferencesUrl = emailUrl.EmailPreferences(user.ApiKey, user.PreferredLanguage);

            templateContext.SetValue("app", app);
            templateContext.SetValue("user", user);
            templateContext.SetValue("notifications", emailNotifications);
            templateContext.SetValue("preferencesUrl", emailPreferencesUrl);

            return new Context { TemplateContext = templateContext, App = app, User = user, Jobs = jobs.ToList() };
        }

        public void AddError(string message, EmailTemplateType template, int? line = -1, int? column = -1)
        {
            Errors.Add(new EmailFormattingError(message, template, line ?? -1, column ?? -1));
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

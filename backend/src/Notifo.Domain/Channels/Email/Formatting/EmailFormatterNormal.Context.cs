// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Apps;
using Notifo.Domain.Resources;
using Notifo.Domain.Users;

#pragma warning disable RECS0082 // Parameter has the same name as a member and hides it
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Notifo.Domain.Channels.Email.Formatting
{
    public sealed partial class EmailFormatterNormal
    {
        private sealed record Context(Dictionary<string, string?> Properties, IReadOnlyList<EmailJob> Jobs, App App, User User)
        {
            public List<EmailFormattingError>? Errors { get; private set; }

            public void AddError(string message, EmailTemplateType template, int? line = -1, int? column = -1)
            {
                Errors ??= new List<EmailFormattingError>();
                Errors.Add(new EmailFormattingError(message, template, line ?? -1, column ?? -1));
            }

            public static Context Create(IReadOnlyList<EmailJob> jobs, App app, User user,
                IEmailUrl emailUrl)
            {
                var notification = jobs[0].Notification;

                var formattingProperties = new Dictionary<string, string?>(StringComparer.InvariantCulture)
                {
                    ["app.name"] = app.Name,
                    ["user.name"] = user.FullName,
                    ["user.nameFull"] = user.FullName,
                    ["user.fullName"] = user.FullName,
                    ["user.email"] = user.EmailAddress,
                    ["user.emailAddress"] = user.EmailAddress,
                    ["notification.subject"] = notification.Formatting.Subject,
                    ["notification.body"] = notification.Formatting.Body,
                    ["preferencesUrl"] = emailUrl.EmailPreferences(user.ApiKey, user.PreferredLanguage)
                };

                foreach (var job in jobs)
                {
                    var properties = job.Notification.Properties;

                    if (properties != null)
                    {
                        foreach (var (key, value) in properties)
                        {
                            formattingProperties[$"notification.custom.{key}"] = value;
                        }
                    }
                }

                return new Context(formattingProperties, jobs, app, user);
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

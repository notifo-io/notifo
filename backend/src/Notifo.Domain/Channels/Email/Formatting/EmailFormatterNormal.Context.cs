// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Apps;
using Notifo.Domain.Resources;
using Notifo.Domain.Users;

namespace Notifo.Domain.Channels.Email.Formatting
{
    public sealed partial class EmailFormatterNormal
    {
        private sealed class Context
        {
            public App App { get; private init; }

            public List<EmailFormattingError> Errors { get; } = new List<EmailFormattingError>();

            public List<EmailJob> Jobs { get; private init; }

            public Dictionary<string, string?> Properties { get; private init; }

            public User User { get; private init; }

            public string? EmailAddress => User.EmailAddress;

            private Context()
            {
            }

            public static Context Create(IReadOnlyList<EmailJob> jobs, App app, User user, IEmailUrl emailUrl)
            {
                var notification = jobs[0].Notification;

                var properties = new Dictionary<string, string?>(StringComparer.InvariantCulture)
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
                    var jobProperties = job.Notification.Properties;

                    if (jobProperties != null)
                    {
                        foreach (var (key, value) in jobProperties)
                        {
                            properties[$"notification.custom.{key}"] = value;
                        }
                    }
                }

                return new Context { Properties = properties, App = app, User = user, Jobs = jobs.ToList() };
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
}

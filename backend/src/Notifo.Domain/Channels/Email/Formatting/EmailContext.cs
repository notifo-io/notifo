// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Apps;
using Notifo.Domain.Resources;
using Notifo.Domain.Users;
using Notifo.Domain.Utils;

namespace Notifo.Domain.Channels.Email.Formatting;

internal sealed class EmailContext : Dictionary<string, object>
{
    public App App { get; private init; }

    public List<EmailFormattingError> Errors { get; } = new List<EmailFormattingError>();

    public List<EmailJob> Jobs { get; private init; }

    public User User { get; private init; }

    private EmailContext()
    {
    }

    public static EmailContext Create(IReadOnlyList<EmailJob> jobs, App app, User user, IImageFormatter imageFormatter, IEmailUrl emailUrl)
    {
        var emailNotifications = jobs.Select(x => new EmailNotification(x.Notification, x.ConfigurationId, imageFormatter)).ToArray();
        var emailPreferencesUrl = emailUrl.EmailPreferences(user.ApiKey, user.PreferredLanguage);

        var context = new EmailContext { App = app, User = user, Jobs = jobs.ToList() };

        context["app"] = app;
        context["preferencesUrl"] = emailPreferencesUrl;
        context["notification"] = emailNotifications[0];
        context["notifications"] = emailNotifications;
        context["user"] = user;

        foreach (var job in jobs)
        {
            var jobProperties = job.Notification.Properties;

            if (jobProperties != null)
            {
                foreach (var (key, value) in jobProperties)
                {
                    context[$"notification.custom.{key}"] = value;
                }
            }
        }

        return context;
    }

    public void AddError(EmailTemplateType type, TemplateError error)
    {
        Errors.Add(new EmailFormattingError(type, error));
    }

    public void AddError(EmailTemplateType type, string error)
    {
        Errors.Add(new EmailFormattingError(type, new TemplateError(error)));
    }

    public void ValidateTemplate(string? template, EmailTemplateType type)
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            return;
        }

        if (Jobs.Any(x => !template.Contains(x.Notification.Formatting.Subject, StringComparison.OrdinalIgnoreCase)))
        {
            AddError(type, Texts.Email_TemplateInvalid);
        }
    }
}

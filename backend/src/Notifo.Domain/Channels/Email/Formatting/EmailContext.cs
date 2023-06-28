// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Fluid;
using Notifo.Domain.Apps;
using Notifo.Domain.Integrations;
using Notifo.Domain.Liquid;
using Notifo.Domain.Resources;
using Notifo.Domain.Users;
using Notifo.Domain.Utils;

namespace Notifo.Domain.Channels.Email.Formatting;

internal sealed class EmailContext
{
    public App App { get; private init; }

    public LiquidContext Liquid { get; private init; }

    public List<EmailFormattingError> Errors { get; } = new List<EmailFormattingError>();

    public List<EmailJob> Jobs { get; private init; }

    public User User { get; private init; }

    private EmailContext()
    {
    }

    public static EmailContext Create(IReadOnlyList<EmailJob> jobs, App app, User user, IImageFormatter imageFormatter, IEmailUrl emailUrl)
    {
        var emailPreferencesUrl = emailUrl.EmailPreferences(user.ApiKey, user.PreferredLanguage);

        var liquidContext =
            LiquidContext.Create(
                jobs.Select(x => (x.Notification, x.ConfigurationId)).ToList(),
                app,
                user,
                Providers.Email,
                "EmailSmall",
                "EmailLarge",
                imageFormatter);

        liquidContext.SetValue("preferencesUrl", emailUrl.EmailPreferences(user.ApiKey, user.PreferredLanguage));

        return new EmailContext { App = app, User = user, Jobs = jobs.ToList(), Liquid = liquidContext };
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

        if (Jobs.Exists(x => !template.Contains(x.Notification.Formatting.Subject, StringComparison.OrdinalIgnoreCase)))
        {
            AddError(type, Texts.Email_TemplateInvalid);
        }
    }
}

// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FluentValidation;
using Notifo.Domain.Apps;
using Notifo.Domain.Integrations;
using Notifo.Domain.Liquid;
using Notifo.Domain.Users;
using Notifo.Domain.Utils;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Reflection.Internal;
using Notifo.Infrastructure.Validation;

namespace Notifo.Domain.Channels.Sms;

public sealed class SmsFormatter : ISmsFormatter
{
    private readonly string defaultText;
    private readonly IImageFormatter imageFormatter;

    private sealed class Validator : AbstractValidator<SmsTemplate>
    {
        public Validator()
        {
            RuleFor(x => x.Text).NotNull().NotEmpty();
        }
    }

    public SmsFormatter(IImageFormatter imageFormatter)
    {
        defaultText = GetType().Assembly.GetManifestResourceString($"Notifo.Domain.Channels.Sms.DefaultTemplate.liquid.text");

        this.imageFormatter = imageFormatter;
    }

    public ValueTask<SmsTemplate> CreateInitialAsync(
        CancellationToken ct = default)
    {
        var template = new SmsTemplate
        {
            Text = defaultText
        };

        return new ValueTask<SmsTemplate>(template);
    }

    public ValueTask ParseAsync(SmsTemplate input, bool strict,
        CancellationToken ct = default)
    {
        Validate<Validator>.It(input);

        return default;
    }

    public (string Result, List<TemplateError>? Errors) Format(SmsTemplate? template, SmsJob job, App app, User user)
    {
        Guard.NotNull(job);
        Guard.NotNull(app);
        Guard.NotNull(user);

        if (template == null)
        {
            return (job.Notification.Formatting.Subject, null);
        }

        var context = LiquidContext.Create(
            Enumerable.Repeat((job.Notification, job.ConfigurationId), 1), app, user,
            Providers.Sms,
            "SmsSmall",
            "SmsLarge",
            imageFormatter);

        var (result, errors) = context.Render(template.Text, false);

        if (string.IsNullOrWhiteSpace(result))
        {
            result = job.Notification.Formatting.Subject;
        }

        return (result, errors);
    }
}

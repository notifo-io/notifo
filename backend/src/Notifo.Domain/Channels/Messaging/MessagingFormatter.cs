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

namespace Notifo.Domain.Channels.Messaging;

public sealed class MessagingFormatter : IMessagingFormatter
{
    private readonly string defaultText;
    private readonly IImageFormatter imageFormatter;

    private sealed class Validator : AbstractValidator<MessagingTemplate>
    {
        public Validator()
        {
            RuleFor(x => x.Text).NotNull().NotEmpty();
        }
    }

    public MessagingFormatter(IImageFormatter imageFormatter)
    {
        defaultText = GetType().Assembly.GetManifestResourceString($"Notifo.Domain.Channels.Messaging.DefaultTemplate.liquid.text");

        this.imageFormatter = imageFormatter;
    }

    public ValueTask<MessagingTemplate> CreateInitialAsync(
        CancellationToken ct = default)
    {
        var template = new MessagingTemplate
        {
            Text = defaultText
        };

        return new ValueTask<MessagingTemplate>(template);
    }

    public ValueTask<MessagingTemplate> ParseAsync(MessagingTemplate input, bool strict,
        CancellationToken ct = default)
    {
        Validate<Validator>.It(input);

        return new ValueTask<MessagingTemplate>(input);
    }

    public (string Result, List<TemplateError>? Errors) Format(MessagingTemplate? template, MessagingJob job, App app, User user)
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
            Providers.Messaging,
            "MessagingSmall",
            "MessagingLarge",
            imageFormatter);

        var (result, errors) = context.Render(template.Text, false);

        if (string.IsNullOrWhiteSpace(result))
        {
            result = job.Notification.Formatting.Subject;
        }

        return (result, errors);
    }
}

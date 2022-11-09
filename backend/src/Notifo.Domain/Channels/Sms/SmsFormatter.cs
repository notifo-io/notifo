// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FluentValidation;
using Notifo.Domain.Utils;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Validation;

namespace Notifo.Domain.Channels.Sms;

public sealed class SmsFormatter : ISmsFormatter
{
    private sealed class Validator : AbstractValidator<SmsTemplate>
    {
        public Validator()
        {
            RuleFor(x => x.Text).NotNull().NotEmpty();
        }
    }

    public ValueTask<SmsTemplate> CreateInitialAsync(string? kind = null,
        CancellationToken ct = default)
    {
        var template = new SmsTemplate
        {
            Text = "{{ notification.subject }}"
        };

        return new ValueTask<SmsTemplate>(template);
    }

    public ValueTask<SmsTemplate> ParseAsync(SmsTemplate input, bool strict,
        CancellationToken ct = default)
    {
        Validate<Validator>.It(input);

        return new ValueTask<SmsTemplate>(input);
    }

    public string Format(SmsTemplate? template, string text)
    {
        Guard.NotNullOrEmpty(text);

        if (template == null)
        {
            return text;
        }

        var properties = new Dictionary<string, string?>
        {
            ["notification.subject"] = text
        };

        return template.Text.Format(properties);
    }
}

﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FluentValidation;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Validation;

namespace Notifo.Domain.Channels.Sms
{
    public sealed class SmsFormatter : ISmsFormatter
    {
        private const string NotificationPlaceholder = "{{notification.subject}}";

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
                Text = NotificationPlaceholder
            };

            return new ValueTask<SmsTemplate>(template);
        }

        public ValueTask<SmsTemplate> ParseAsync(SmsTemplate input,
            CancellationToken ct = default)
        {
            Validate<Validator>.It(input);

            return new ValueTask<SmsTemplate>(input);
        }

        public string Format(SmsTemplate? template, string text)
        {
            Guard.NotNull(template);
            Guard.NotNullOrEmpty(text);

            if (template == null)
            {
                return MaxLength(text, 140);
            }

            var templateLength = template.Text.Replace(NotificationPlaceholder, string.Empty, StringComparison.OrdinalIgnoreCase).Length;

            var maxLength = 140 - templateLength;

            return template.Text.Replace(NotificationPlaceholder, MaxLength(text, maxLength), StringComparison.OrdinalIgnoreCase);
        }

        private static string MaxLength(string source, int maxLength)
        {
            if (source.Length > maxLength)
            {
                return source[..maxLength];
            }

            return source;
        }
    }
}

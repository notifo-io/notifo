﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FluentValidation;
using Notifo.Domain.UserNotifications;
using Notifo.Domain.Utils;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Validation;

namespace Notifo.Domain.Channels.Messaging;

public sealed class MessagingFormatter : IMessagingFormatter
{
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
        this.imageFormatter = imageFormatter;
    }

    public ValueTask<MessagingTemplate> CreateInitialAsync(
        CancellationToken ct = default)
    {
        var template = new MessagingTemplate
        {
            Text = "{{notification.subject}}"
        };

        return new ValueTask<MessagingTemplate>(template);
    }

    public ValueTask ParseAsync(MessagingTemplate input, bool strict,
        CancellationToken ct = default)
    {
        Validate<Validator>.It(input);

        return default;
    }

    public string Format(MessagingTemplate? template, BaseUserNotification notification)
    {
        Guard.NotNull(notification);

        if (template == null)
        {
            return notification.Formatting.Subject;
        }

        var formattingProperties = new Dictionary<string, string?>
        {
            ["notification.body"] = notification.BodyWithLink(),
            ["notification.confirmText"] = notification.ConfirmText(),
            ["notification.confirmUrl"] = notification.ConfirmUrl(),
            ["notification.imageLarge"] = notification.ImageLarge(imageFormatter, "MessagingLarge"),
            ["notification.imageSmall"] = notification.ImageSmall(imageFormatter, "MessagingSmall"),
            ["notification.subject"] = notification.Subject()
        };

        var properties = notification.Properties;

        if (properties != null)
        {
            foreach (var (key, value) in properties)
            {
                formattingProperties[$"notification.custom.{key}"] = value;
            }
        }

        return template.Text.Format(formattingProperties);
    }
}

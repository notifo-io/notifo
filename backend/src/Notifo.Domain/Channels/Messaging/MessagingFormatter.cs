// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using Notifo.Domain.UserNotifications;
using Notifo.Domain.Utils;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Validation;

namespace Notifo.Domain.Channels.Messaging
{
    public sealed class MessagingFormatter : IMessagingFormatter
    {
        private const string PlaceholderSubject = "{{notification.subject}}";
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

        public ValueTask<MessagingTemplate> CreateInitialAsync()
        {
            var template = new MessagingTemplate
            {
                Text = PlaceholderSubject
            };

            return new ValueTask<MessagingTemplate>(template);
        }

        public ValueTask<MessagingTemplate> ParseAsync(MessagingTemplate input)
        {
            Validate<Validator>.It(input);

            return new ValueTask<MessagingTemplate>(input);
        }

        public string Format(MessagingTemplate? template, BaseUserNotification notification)
        {
            Guard.NotNull(template, nameof(template));
            Guard.NotNull(notification, nameof(notification));

            if (template == null)
            {
                return notification.Formatting.Subject;
            }

            var notificationProperties = new Dictionary<string, string?>
            {
                ["notification.body"] = notification.Body(),
                ["notification.confirmText"] = notification.ConfirmText(),
                ["notification.confirmUrl"] = notification.ConfirmUrl(),
                ["notification.imageLarge"] = notification.ImageLarge(imageFormatter, "MessagingLarge"),
                ["notification.imageSmall"] = notification.ImageSmall(imageFormatter, "MessagingSmall"),
                ["notification.subject"] = notification.Subject()
            };

            return template.Text.Format(notificationProperties);
        }
    }
}

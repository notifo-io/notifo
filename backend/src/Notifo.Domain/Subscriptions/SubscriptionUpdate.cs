// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FluentValidation;
using Notifo.Infrastructure.Validation;

namespace Notifo.Domain.Subscriptions
{
    public sealed class SubscriptionUpdate
    {
        public string UserId { get; set; }

        public string TopicPrefix { get; set; }

        public NotificationSettings TopicSettings { get; } = new NotificationSettings();

        private sealed class Validator : AbstractValidator<SubscriptionUpdate>
        {
            public Validator()
            {
                RuleFor(x => x.TopicPrefix).Topic();
                RuleFor(x => x.UserId).NotNull();
            }
        }

        public void Validate()
        {
            Validate<Validator>.It(this);
        }
    }
}

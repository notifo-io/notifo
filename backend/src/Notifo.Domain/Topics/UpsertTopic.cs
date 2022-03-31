// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FluentValidation;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Collections;
using Notifo.Infrastructure.Texts;
using Notifo.Infrastructure.Validation;

namespace Notifo.Domain.Topics
{
    public sealed class UpsertTopic : ICommand<Topic>
    {
        public LocalizedText? Name { get; set; }

        public LocalizedText? Description { get; set; }

        public ReadonlyDictionary<string, TopicChannel>? Channels { get; set; }

        public bool CanCreate => true;

        private sealed class Validator : AbstractValidator<UpsertTopic>
        {
            public Validator()
            {
                RuleFor(x => x.Name).NotNull().NotEmpty();
            }
        }

        public ValueTask<Topic?> ExecuteAsync(Topic target, IServiceProvider serviceProvider, CancellationToken ct)
        {
            Validate<Validator>.It(this);

            var newTopic = target with
            {
                IsExplicit = true
            };

            if (Name != null)
            {
                newTopic = newTopic with
                {
                    Name = Name
                };
            }

            if (Description != null)
            {
                newTopic = newTopic with
                {
                    Description = Description
                };
            }

            if (Channels != null)
            {
                newTopic = newTopic with
                {
                    Channels = Channels
                };
            }

            return new ValueTask<Topic?>(newTopic);
        }
    }
}

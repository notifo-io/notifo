// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FluentValidation;
using Notifo.Domain.Utils;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Collections;
using Notifo.Infrastructure.Texts;
using Notifo.Infrastructure.Validation;

namespace Notifo.Domain.Topics;

public sealed class UpsertTopic : TopicCommand
{
    public LocalizedText? Name { get; set; }

    public LocalizedText? Description { get; set; }

    public ReadonlyDictionary<string, TopicChannel>? Channels { get; set; }

    public bool? ShowAutomatically { get; set; }

    public override bool CanCreate => true;

    private sealed class Validator : AbstractValidator<UpsertTopic>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty();
        }
    }

    public override ValueTask<Topic?> ExecuteAsync(Topic target, IServiceProvider serviceProvider,
        CancellationToken ct)
    {
        Validate<Validator>.It(this);

        var newTopic = target with
        {
            IsExplicit = true
        };

        if (Is.Changed(Name, target.Name))
        {
            newTopic = newTopic with
            {
                Name = Name.Trim()
            };
        }

        if (Is.Changed(Description, target.Description))
        {
            newTopic = newTopic with
            {
                Description = Description.Trim()
            };
        }

        if (Is.Changed(ShowAutomatically, target.ShowAutomatically))
        {
            newTopic = newTopic with
            {
                ShowAutomatically = ShowAutomatically.Value
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

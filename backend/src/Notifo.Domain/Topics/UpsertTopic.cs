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

public sealed class UpsertTopic : ICommand<Topic>
{
    public LocalizedText? Name { get; set; }

    public LocalizedText? Description { get; set; }

    public ReadonlyDictionary<string, TopicChannel>? Channels { get; set; }

    public bool? ShowAutomatically { get; set; }

    public bool CanCreate => true;

    private sealed class Validator : AbstractValidator<UpsertTopic>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty();
        }
    }

    public ValueTask<Topic?> ExecuteAsync(Topic topic, IServiceProvider serviceProvider,
        CancellationToken ct)
    {
        Validate<Validator>.It(this);

        var newTopic = topic with
        {
            IsExplicit = true
        };

        if (Is.Changed(Name, topic.Name))
        {
            newTopic = newTopic with
            {
                Name = Name.Trim()
            };
        }

        if (Is.Changed(Description, topic.Description))
        {
            newTopic = newTopic with
            {
                Description = Description.Trim()
            };
        }

        if (Is.Changed(ShowAutomatically, topic.ShowAutomatically))
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

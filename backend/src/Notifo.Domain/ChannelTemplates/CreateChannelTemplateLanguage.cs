// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Collections;
using Notifo.Infrastructure.Validation;

namespace Notifo.Domain.ChannelTemplates;

public sealed class CreateChannelTemplateLanguage<T> : ChannelTemplateCommand<T>
{
    public string Language { get; set; }

    private sealed class Validator : AbstractValidator<CreateChannelTemplateLanguage<T>>
    {
        public Validator()
        {
            RuleFor(x => x.Language).NotNull().Language();
        }
    }

    public override async ValueTask<ChannelTemplate<T>?> ExecuteAsync(ChannelTemplate<T> target, IServiceProvider serviceProvider,
        CancellationToken ct)
    {
        Validate<Validator>.It(this);

        var channelFactory = serviceProvider.GetRequiredService<IChannelTemplateFactory<T>>();

        if (target.Languages.ContainsKey(Language))
        {
            throw new DomainObjectConflictException(Language);
        }

        var channelInstance = await channelFactory.CreateInitialAsync(target.Kind, ct);

        var newTemplate = target with
        {
            Languages = target.Languages.Set(Language, channelInstance)
        };

        return newTemplate;
    }
}

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

public sealed class UpdateChannelTemplateLanguage<T> : ChannelTemplateCommand<T>
{
    public string Language { get; init; }

    public T Template { get; init; }

    private sealed class Validator : AbstractValidator<UpdateChannelTemplateLanguage<T>>
    {
        public Validator()
        {
            RuleFor(x => x.Language).NotNull().Language();
            RuleFor(x => x.Template).NotNull();
        }
    }

    public override async ValueTask<ChannelTemplate<T>?> ExecuteAsync(ChannelTemplate<T> target, IServiceProvider serviceProvider,
        CancellationToken ct)
    {
        Validate<Validator>.It(this);

        var factory = serviceProvider.GetRequiredService<IChannelTemplateFactory<T>>();

        var newLanguage = await factory.ParseAsync(Template, true, ct);
        var newTemplate = target with
        {
            Languages = target.Languages.Set(Language, newLanguage)
        };

        return newTemplate;
    }
}

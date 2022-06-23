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

namespace Notifo.Domain.ChannelTemplates
{
    public sealed class CreateChannelTemplateLanguage<T> : ICommand<ChannelTemplate<T>>
    {
        public string Language { get; set; }

        private sealed class Validator : AbstractValidator<CreateChannelTemplateLanguage<T>>
        {
            public Validator()
            {
                RuleFor(x => x.Language).NotNull().Language();
            }
        }

        public async ValueTask<ChannelTemplate<T>?> ExecuteAsync(ChannelTemplate<T> template, IServiceProvider serviceProvider,
            CancellationToken ct)
        {
            Validate<Validator>.It(this);

            var channelFactory = serviceProvider.GetRequiredService<IChannelTemplateFactory<T>>();

            if (template.Languages.ContainsKey(Language))
            {
                throw new DomainObjectConflictException(Language);
            }

            var channelInstance = await channelFactory.CreateInitialAsync(template.Kind, ct);

            var newTemplate = template with
            {
                Languages = template.Languages.Set(Language, channelInstance)
            };

            return newTemplate;
        }
    }
}

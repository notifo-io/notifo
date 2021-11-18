// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FluentValidation;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Collections;
using Notifo.Infrastructure.Validation;

namespace Notifo.Domain.ChannelTemplates
{
    public sealed class DeleteChannelTemplateLanguage<T> : ICommand<ChannelTemplate<T>>
    {
        public string Language { get; set; }

        private sealed class Validator : AbstractValidator<DeleteChannelTemplateLanguage<T>>
        {
            public Validator()
            {
                RuleFor(x => x.Language).NotNull().Language();
            }
        }

        public ValueTask<ChannelTemplate<T>?> ExecuteAsync(ChannelTemplate<T> template, IServiceProvider serviceProvider,
            CancellationToken ct)
        {
            Validate<Validator>.It(this);

            if (!template.Languages.ContainsKey(Language))
            {
                return default;
            }

            var newTemplate = template with
            {
                Languages = template.Languages.Where(x => x.Key != Language).ToReadonlyDictionary()
            };

            return new ValueTask<ChannelTemplate<T>?>(newTemplate);
        }
    }
}

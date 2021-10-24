// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Collections;
using Notifo.Infrastructure.Validation;

namespace Notifo.Domain.ChannelTemplates
{
    public sealed class UpdateChannelTemplateLanguage<T> : ICommand<ChannelTemplate<T>>
    {
        public string Language { get; set; }

        public T Template { get; set; }

        private sealed class Validator : AbstractValidator<UpdateChannelTemplateLanguage<T>>
        {
            public Validator()
            {
                RuleFor(x => x.Language).NotNull().Language();
                RuleFor(x => x.Template).NotNull();
            }
        }

        public async ValueTask<ChannelTemplate<T>?> ExecuteAsync(ChannelTemplate<T> template, IServiceProvider serviceProvider,
            CancellationToken ct)
        {
            Validate<Validator>.It(this);

            var factory = serviceProvider.GetRequiredService<IChannelTemplateFactory<T>>();

            var newLanguages = new Dictionary<string, T>(template.Languages)
            {
                [Language] = await factory.ParseAsync(Template)
            };

            var newTemplate = template with
            {
                Languages = newLanguages.ToReadonlyDictionary()
            };

            return newTemplate;
        }
    }
}

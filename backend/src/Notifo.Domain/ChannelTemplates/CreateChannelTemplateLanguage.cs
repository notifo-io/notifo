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

            var factory = serviceProvider.GetRequiredService<IChannelTemplateFactory<T>>();

            if (template.Languages.ContainsKey(Language))
            {
                throw new DomainObjectConflictException(Language);
            }

            var newLanguages = new Dictionary<string, T>(template.Languages)
            {
                [Language] = await factory.CreateInitialAsync()
            };

            var newTemplate = template with
            {
                Languages = newLanguages.ToReadonlyDictionary()
            };

            return newTemplate;
        }
    }
}

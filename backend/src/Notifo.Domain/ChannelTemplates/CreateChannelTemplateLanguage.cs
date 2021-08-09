// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Notifo.Infrastructure;
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

        public async Task<bool> ExecuteAsync(ChannelTemplate<T> template, IServiceProvider serviceProvider,
            CancellationToken ct)
        {
            Validate<Validator>.It(this);

            var factory = serviceProvider.GetRequiredService<IChannelTemplateFactory<T>>();

            if (template.Languages.ContainsKey(Language))
            {
                throw new DomainObjectConflictException(Language);
            }

            template.Languages[Language] = await factory.CreateInitialAsync();

            return true;
        }
    }
}

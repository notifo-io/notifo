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
using NodaTime;
using Notifo.Domain.Channels.Email;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Validation;

namespace Notifo.Domain.ChannelTemplates
{
    public sealed class UpdateChannelTemplate<T> : ICommand<ChannelTemplate<T>>
    {
        public string? Language { get; set; }

        public string? Name { get; set; }

        public bool Primary { get; set; }

        public T? Template { get; set; }

        private sealed class Validator : AbstractValidator<UpdateChannelTemplate<T>>
        {
            public Validator()
            {
                RuleFor(x => x.Template).NotNull().When(x => x.Language != null);
            }
        }

        private sealed class EmailTemplateValidator : AbstractValidator<EmailTemplate>
        {
            public EmailTemplateValidator()
            {
                RuleFor(x => x.BodyHtml).NotNull().NotEmpty();
                RuleFor(x => x.BodyText);
                RuleFor(x => x.Subject).NotNull().NotEmpty();
            }
        }

        public async Task<bool> ExecuteAsync(ChannelTemplate<T> template, IServiceProvider serviceProvider,
            CancellationToken ct)
        {
            Validate<Validator>.It(this);

            var factory = serviceProvider.GetRequiredService<IChannelTemplateFactory<T>>();

            var clock = serviceProvider.GetRequiredService<IClock>();

            if (Language != null)
            {
                template.Languages[Language] = await factory.ParseAsync(Template!);
            }
            else
            {
                template.Name = Name;
                template.Primary = Primary;
            }

            template.LastUpdate = clock.GetCurrentInstant();

            return true;
        }
    }
}

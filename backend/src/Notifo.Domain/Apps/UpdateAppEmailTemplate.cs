// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Notifo.Domain.Channels.Email;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Validation;
using ValidationException = Notifo.Infrastructure.Validation.ValidationException;

namespace Notifo.Domain.Apps
{
    public sealed class UpdateAppEmailTemplate : ICommand<App>
    {
        public string Language { get; set; }

        public EmailTemplate EmailTemplate { get; set; }

        private sealed class Validator : AbstractValidator<UpdateAppEmailTemplate>
        {
            public Validator()
            {
                RuleFor(x => x.Language).NotNull().Language();
                RuleFor(x => x.EmailTemplate).NotNull().SetValidator(new EmailTemplateValidator());
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

        public async Task<bool> ExecuteAsync(App app, IServiceProvider serviceProvider, CancellationToken ct)
        {
            Validate<Validator>.It(this);

            if (!app.Languages.Contains(Language))
            {
                var error = new ValidationError("Language is not supported.", nameof(Language));

                throw new ValidationException(error);
            }

            var emailFormatter = serviceProvider.GetRequiredService<IEmailFormatter>()!;

            app.EmailTemplates[Language] = await emailFormatter.ParseAsync(EmailTemplate);

            return true;
        }
    }
}

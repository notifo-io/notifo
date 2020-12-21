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
using Notifo.Infrastructure;
using Notifo.Infrastructure.Validation;

namespace Notifo.Domain.Apps
{
    public sealed class DeleteAppEmailTemplate : ICommand<App>
    {
        public string Language { get; set; }

        private sealed class Validator : AbstractValidator<DeleteAppEmailTemplate>
        {
            public Validator()
            {
                RuleFor(x => x.Language).NotNull().Language();
            }
        }

        public Task ExecuteAsync(App app, IServiceProvider serviceProvider, CancellationToken ct)
        {
            Validate<Validator>.It(this);

            app.EmailTemplates.Remove(Language);

            return Task.CompletedTask;
        }
    }
}

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
using Notifo.Domain.Resources;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Identity;
using Notifo.Infrastructure.Validation;
using ValidationException = Notifo.Infrastructure.Validation.ValidationException;

namespace Notifo.Domain.Apps
{
    public sealed class AddContributor : ICommand<App>
    {
        public string UserId { get; set; }

        public string Email { get; set; }

        public string Role { get; set; }

        private sealed class Validator : AbstractValidator<AddContributor>
        {
            public Validator()
            {
                RuleFor(x => x.Email).NotNull();
                RuleFor(x => x.Role).NotNull().Role();
            }
        }

        public async Task ExecuteAsync(App app, IServiceProvider serviceProvider, CancellationToken ct)
        {
            Validate<Validator>.It(this);

            var userManager = serviceProvider.GetRequiredService<IUserResolver>();

            var id = await userManager.GetOrAddUserAsync(Email);

            if (id == null)
            {
                var error = new ValidationError(Texts.Apps_UserNotFound, nameof(Email));

                throw new ValidationException(error);
            }

            if (string.Equals(id, UserId, StringComparison.OrdinalIgnoreCase))
            {
                throw new DomainException(Texts.App_CannotUpdateYourself);
            }

            app.Contributors[id] = Role;
        }
    }
}

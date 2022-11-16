// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Notifo.Domain.Identity;
using Notifo.Domain.Resources;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Collections;
using Notifo.Infrastructure.Validation;
using ValidationException = Notifo.Infrastructure.Validation.ValidationException;

namespace Notifo.Domain.Apps;

public sealed class AddContributor : AppCommand
{
    public string EmailOrId { get; set; }

    public string Role { get; set; } = NotifoRoles.AppAdmin;

    private sealed class Validator : AbstractValidator<AddContributor>
    {
        public Validator()
        {
            RuleFor(x => x.EmailOrId).NotNull();
            RuleFor(x => x.Role).NotNull().Role();
        }
    }

    public override async ValueTask<App?> ExecuteAsync(App target, IServiceProvider serviceProvider,
        CancellationToken ct)
    {
        Validate<Validator>.It(this);

        var userResolver = serviceProvider.GetRequiredService<IUserResolver>();

        var (user, _) = await userResolver.CreateUserIfNotExistsAsync(EmailOrId, ct: ct);

        if (user == null)
        {
            var error = new ValidationError(Texts.Apps_UserNotFound, nameof(EmailOrId));

            throw new ValidationException(error);
        }

        if (string.Equals(user.Id, PrincipalId, StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException(Texts.App_CannotUpdateYourself);
        }

        EmailOrId = user.Id;

        var newApp = target with
        {
            Contributors = target.Contributors.Set(user.Id, Role)
        };

        return newApp;
    }
}

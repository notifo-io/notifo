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

    public async ValueTask<App?> ExecuteAsync(App app, IServiceProvider serviceProvider,
        CancellationToken ct)
    {
        Validate<Validator>.It(this);

        var userResolver = serviceProvider.GetRequiredService<IUserResolver>();

        var (user, _) = await userResolver.CreateUserIfNotExistsAsync(Email, ct: ct);

        if (user == null)
        {
            var error = new ValidationError(Texts.Apps_UserNotFound, nameof(Email));

            throw new ValidationException(error);
        }

        if (string.Equals(user.Id, UserId, StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException(Texts.App_CannotUpdateYourself);
        }

        var newApp = app with
        {
            Contributors = app.Contributors.Set(user.Id, Role)
        };

        return newApp;
    }
}

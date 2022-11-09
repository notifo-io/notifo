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

namespace Notifo.Domain.Users;

public sealed class SetUserSystemProperty : ICommand<User>
{
    public string PropertyKey { get; set; }

    public string? PropertyValue { get; set; }

    private sealed class Validator : AbstractValidator<SetUserSystemProperty>
    {
        public Validator()
        {
            RuleFor(x => x.PropertyKey).NotNull().NotEmpty();
        }
    }

    public ValueTask<User?> ExecuteAsync(User user, IServiceProvider serviceProvider,
        CancellationToken ct)
    {
        Validate<Validator>.It(this);

        var newUser = user;

        var existing = user.SystemProperties?.GetValueOrDefault(PropertyKey);

        if (!string.Equals(existing, PropertyValue, StringComparison.Ordinal))
        {
            var newProperties = user.SystemProperties;

            if (PropertyValue == null)
            {
                newProperties = newProperties.Remove(PropertyKey);
            }
            else
            {
                newProperties = newProperties.Set(PropertyKey, PropertyValue);
            }

            newUser = newUser with
            {
                SystemProperties = newProperties
            };
        }

        return new ValueTask<User?>(newUser);
    }
}

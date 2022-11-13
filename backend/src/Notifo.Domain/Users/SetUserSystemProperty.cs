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

public sealed class SetUserSystemProperty : UserCommand
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

    public override ValueTask<User?> ExecuteAsync(User target, IServiceProvider serviceProvider,
        CancellationToken ct)
    {
        Validate<Validator>.It(this);

        var newUser = target;

        var existing = target.SystemProperties?.GetValueOrDefault(PropertyKey);

        if (!string.Equals(existing, PropertyValue, StringComparison.Ordinal))
        {
            var newProperties = target.SystemProperties;

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

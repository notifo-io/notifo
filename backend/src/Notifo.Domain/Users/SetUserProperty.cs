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

namespace Notifo.Domain.Users
{
    public sealed class SetUserProperty : ICommand<User>
    {
        public string PropertyKey { get; set; }

        public string? PropertyValue { get; set; }

        private sealed class Validator : AbstractValidator<SetUserProperty>
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

            var existing = user.Properties.GetValueOrDefault(PropertyKey);

            if (!string.Equals(existing, PropertyValue, StringComparison.Ordinal))
            {
                var newProperties = new Dictionary<string, string>(user.Properties);

                if (PropertyValue == null)
                {
                    newProperties.Remove(PropertyKey);
                }
                else
                {
                    newProperties[PropertyKey] = PropertyValue;
                }

                newUser = newUser with
                {
                    Properties = new ReadonlyDictionary<string, string>(newProperties)
                };
            }

            return new ValueTask<User?>(newUser);
        }
    }
}

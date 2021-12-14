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
        public string Property { get; set; }

        public string? PropertyValue { get; set; }

        private sealed class Validator : AbstractValidator<SetUserProperty>
        {
            public Validator()
            {
                RuleFor(x => x.Property).NotNull().NotEmpty();
            }
        }

        public ValueTask<User?> ExecuteAsync(User user, IServiceProvider serviceProvider,
            CancellationToken ct)
        {
            Validate<Validator>.It(this);

            var newUser = user;

            var existing = user.Properties.GetValueOrDefault(Property);

            if (!string.Equals(existing, PropertyValue, StringComparison.Ordinal))
            {
                var newProperties = new Dictionary<string, string>(user.Properties);

                if (PropertyValue == null)
                {
                    newProperties.Remove(Property);
                }
                else
                {
                    newProperties[Property] = PropertyValue;
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

// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using FluentValidation;

namespace Notifo.Infrastructure.Validation
{
    public static class Validate<T> where T : IValidator, new()
    {
        private static readonly T Instance = new T();

        public static void It<TValue>(TValue value)
        {
            var context = new ValidationContext<TValue>(value);

            var results = Instance.Validate(context);

            if (!results.IsValid)
            {
                var errors = results.Errors.Select(x => new ValidationError(x.ErrorMessage, x.PropertyName)).ToArray();

                throw new ValidationException(errors);
            }
        }
    }
}
// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Options;

namespace Notifo.Infrastructure.Configuration
{
    public sealed class OptionsErrorProvider<T> : IErrorProvider, IValidateOptions<T> where T : class, IValidatableOptions
    {
        private readonly IOptions<T> options;
        private readonly string prefix;

        public OptionsErrorProvider(IOptions<T> options, string prefix)
        {
            this.options = options;

            this.prefix = prefix;
        }

        public IEnumerable<ConfigurationError> GetErrors()
        {
            return GetErrors(options.Value);
        }

        public ValidateOptionsResult Validate(string name, T options)
        {
            var errors = GetErrors(options).ToList();

            if (errors.Count > 0)
            {
                var sb = new StringBuilder();

                foreach (var error in errors)
                {
                    sb.AppendLine(error.ToString());
                }

                return ValidateOptionsResult.Fail(sb.ToString());
            }

            return ValidateOptionsResult.Success;
        }

        private IEnumerable<ConfigurationError> GetErrors(T value)
        {
            foreach (var error in value.Validate())
            {
                if (!string.IsNullOrWhiteSpace(error.Path))
                {
                    yield return new ConfigurationError(error.Message, $"{prefix}:{error.Path}");
                }
                else
                {
                    yield return new ConfigurationError(error.Message, prefix);
                }
            }
        }
    }
}

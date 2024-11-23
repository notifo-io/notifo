// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;

namespace Notifo.Infrastructure.Validation;

[Serializable]
public class ValidationException(IReadOnlyList<ValidationError> errors, Exception? inner = null) : DomainException(FormatMessage(errors), ValidationError, inner)
{
    private const string ValidationError = "VALIDATION_ERROR";

    public IReadOnlyList<ValidationError> Errors { get; } = errors;

    public ValidationException(string error, Exception? inner = null)
        : this(new ValidationError(error), inner)
    {
    }

    public ValidationException(ValidationError error, Exception? inner = null)
        : this(new List<ValidationError> { error }, inner)
    {
    }

    private static string FormatMessage(IReadOnlyList<ValidationError> errors)
    {
        Guard.NotNull(errors);

        var sb = new StringBuilder();

        for (var i = 0; i < errors.Count; i++)
        {
            var error = errors[i]?.Message;

            if (!string.IsNullOrWhiteSpace(error))
            {
                var properties = errors[i].PropertyNames;

                if (properties.Any())
                {
                    var isFirst = true;

                    foreach (var property in properties)
                    {
                        if (!isFirst)
                        {
                            sb.Append(',');
                            sb.Append(' ');
                        }

                        sb.Append(property);
                        isFirst = false;
                    }

                    sb.Append(':');
                    sb.Append(' ');
                }

                sb.Append(error);

                if (!error.EndsWith(".", StringComparison.OrdinalIgnoreCase))
                {
                    sb.Append('.');
                }

                if (i < errors.Count - 1)
                {
                    sb.Append(' ');
                }
            }
        }

        return sb.ToString();
    }
}

// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;
using System.Globalization;
using System.Security;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Notifo.Areas.Api;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Validation;

namespace Notifo.Pipeline;

public static class ApiExceptionConverter
{
    private static readonly Dictionary<int, string> Links = new Dictionary<int, string>
    {
        [400] = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        [401] = "https://tools.ietf.org/html/rfc7235#section-3.1",
        [403] = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
        [404] = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        [406] = "https://tools.ietf.org/html/rfc7231#section-6.5.6",
        [409] = "https://tools.ietf.org/html/rfc7231#section-6.5.8",
        [412] = "https://tools.ietf.org/html/rfc7231#section-6.5.10",
        [415] = "https://tools.ietf.org/html/rfc7231#section-6.5.13",
        [422] = "https://tools.ietf.org/html/rfc4918#section-11.2",
        [500] = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
    };

    public static (ErrorDto Error, bool WellKnown) ToErrorDto(int statusCode, HttpContext? httpContext)
    {
        var error = new ErrorDto { StatusCode = statusCode };

        Enrich(httpContext, error);

        return (error, true);
    }

    public static (ErrorDto Error, bool WellKnown) ToErrorDto(this ProblemDetails problem, HttpContext? httpContext)
    {
        Guard.NotNull(problem);

        var error = CreateError(problem.Status ?? 500, problem.Title);

        Enrich(httpContext, error);

        return (error, true);
    }

    public static (ErrorDto Error, Exception? Unhandled) ToErrorDto(this Exception exception, IStringLocalizer<AppResources> localizer, HttpContext? httpContext)
    {
        Guard.NotNull(exception);

        var result = CreateError(exception, localizer);

        Enrich(httpContext, result.Error);

        return result;
    }

    private static void Enrich(HttpContext? httpContext, ErrorDto error)
    {
        error.TraceId = Activity.Current?.Id ?? httpContext?.TraceIdentifier;

        if (error.StatusCode == 0)
        {
            error.StatusCode = 500;
        }

        error.Type = Links.GetOrDefault(error.StatusCode);
    }

    private static (ErrorDto Error, Exception? Unhandled) CreateError(Exception exception, IStringLocalizer<AppResources> localizer)
    {
        switch (exception)
        {
            case ValidationException ex:
                return (CreateError(400, localizer["ValidationError"].ToString(), ex.ErrorCode, ToErrors(ex.Errors).ToArray()), GetInner(exception));

            case DomainObjectNotFoundException ex:
                return (CreateError(404, ex.ErrorCode), GetInner(exception));

            case DomainObjectDeletedException ex:
                return (CreateError(410, ex.Message, ex.ErrorCode), GetInner(exception));

            case DomainObjectConflictException ex:
                return (CreateError(409, ex.Message, ex.ErrorCode), GetInner(exception));

            case DomainForbiddenException ex:
                return (CreateError(403, ex.Message, ex.ErrorCode), GetInner(exception));

            case DomainException ex:
                return (CreateError(400, ex.Message, ex.ErrorCode), GetInner(exception));

            case SecurityException:
                return (CreateError(403), exception);

            case DecoderFallbackException ex:
                return (CreateError(400, ex.Message), null);

            case BadHttpRequestException ex:
                return (CreateError(ex.StatusCode, ex.Message), null);

            default:
                return (CreateError(500), exception);
        }
    }

    private static Exception? GetInner(Exception exception)
    {
        var current = exception;

        while (current != null)
        {
            if (current is not DomainException)
            {
                return current;
            }

            current = current.InnerException;
        }

        return null;
    }

    private static ErrorDto CreateError(int status, string? message = null, string? errorCode = null, IEnumerable<string>? details = null)
    {
        var error = new ErrorDto { StatusCode = status, Message = message };

        if (!string.IsNullOrWhiteSpace(errorCode))
        {
            error.ErrorCode = errorCode;
        }

        error.Details = details?.ToArray();

        return error;
    }

    public static IEnumerable<string> ToErrors(IEnumerable<ValidationError> errors)
    {
        static string FixPropertyName(string property)
        {
            property = property.Trim();

            if (property.Length == 0)
            {
                return property;
            }

            var prevChar = 0;

            var builder = new StringBuilder(property.Length);

            builder.Append(char.ToLower(property[0], CultureInfo.InvariantCulture));

            foreach (var character in property.Skip(1))
            {
                if (prevChar == '.')
                {
                    builder.Append(char.ToLower(character, CultureInfo.InvariantCulture));
                }
                else
                {
                    builder.Append(character);
                }

                prevChar = character;
            }

            return builder.ToString();
        }

        return errors.Select(e =>
        {
            if (e.PropertyNames?.Any() == true)
            {
                return $"{string.Join(", ", e.PropertyNames.Select(FixPropertyName))}: {e.Message}";
            }
            else
            {
                return e.Message;
            }
        });
    }
}

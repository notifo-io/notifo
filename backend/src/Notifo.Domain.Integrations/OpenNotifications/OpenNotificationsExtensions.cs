// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure;
using Notifo.Infrastructure.Validation;
using OpenNotifications;

namespace Notifo.Domain.Integrations.OpenNotifications;

public static class OpenNotificationsExtensions
{
    public static Dictionary<string, object?> ToProperties(this IReadOnlyDictionary<string, string> properties, IntegrationDefinition definition)
    {
        var result = new Dictionary<string, object?>();

        foreach (var property in definition.Properties)
        {
            object? value;

            if (property.Type == PropertyType.Boolean)
            {
                value = property.GetBoolean(properties);
            }
            else if (property.Type == PropertyType.Number)
            {
                value = property.GetNumber(properties);
            }
            else
            {
                value = property.GetString(properties);
            }

            result[property.Name] = value;
        }

        return result;
    }

    public static RequestContextDto ToContext(this IntegrationContext context)
    {
        var result = new RequestContextDto
        {
            TennantId = context.AppId,
            Trusted = true,
            HostUrl = context.CallbackUrl,
            AuthHeaders = new Dictionary<string, string>
            {
                ["ApiKey"] = context.CallbackToken
            }
        };

        return result;
    }

    public static DeliveryResult ToDeliveryResult(this NotificationStatusDto status)
    {
        switch (status.Status)
        {
            case NotificationStatusDtoStatus.Delivered:
                return DeliveryResult.Handled;
            case NotificationStatusDtoStatus.Failed:
                return DeliveryResult.Failed(status.Errors?.FirstOrDefault()?.Message);
            case NotificationStatusDtoStatus.Sent:
                return DeliveryResult.Sent;
            default:
                return default;
        }
    }

    public static DomainException ToDomainException(this OpenNotificationsException exception)
    {
        if (exception is OpenNotificationsException<ApiErrorDto> typed && exception.StatusCode == 400)
        {
            var errors =
                typed.Result?.Details?.Select(x => new ValidationError(x.Message, GetPropertyNames(x)))?.ToArray() ??
                new[]
                {
                    new ValidationError(typed.Message)
                };

            return new ValidationException(errors);
        }

        return new DomainException(exception.Message);

        static string[] GetPropertyNames(ErrorDto x)
        {
            return x.Field != null ? new[] { x.Field } : Array.Empty<string>();
        }
    }
}

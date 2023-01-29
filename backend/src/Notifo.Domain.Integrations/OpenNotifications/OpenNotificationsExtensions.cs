// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

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
}

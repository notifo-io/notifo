// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Microsoft.AspNetCore.Http;
using Notifo.Domain.Integrations.MessageBird.Implementation;
using Notifo.Domain.Integrations.Resources;
using Notifo.Infrastructure;

namespace Notifo.Domain.Integrations.MessageBird;

public sealed partial class MessageBirdSmsIntegration : ISmsSender, IIntegrationHook
{
    public async Task<DeliveryResult> SendAsync(IntegrationContext context, SmsMessage message,
        CancellationToken ct)
    {
        var to = message.To;
        try
        {
            // Call the phone number and use a local phone number for the user.
            var sms = new Implementation.SmsMessage(
                GetOriginator(context, to),
                to,
                message.Text,
                message.TrackingToken,
                context.WebhookUrl);

            var response = await GetClient(context).SendSmsAsync(sms, ct);

            // Usually an error is received by the error response in the client, but in some cases it was not working properly.
            if (response.Recipients.TotalSentCount != 1)
            {
                var errorMessage = string.Format(CultureInfo.CurrentCulture, Texts.MessageBird_ErrorUnknown, to);

                throw new DomainException(errorMessage);
            }

            // We get the status asynchronously via webhook, therefore we tell the channel not mark the process as completed.
            return DeliveryResult.Sent;
        }
        catch (ArgumentException ex)
        {
            var errorMessage = string.Format(CultureInfo.CurrentCulture, Texts.MessageBird_Error, to, ex.Message);

            throw new DomainException(errorMessage);
        }
    }

    private static string GetOriginator(IntegrationContext context, string to)
    {
        var originatorName = OriginatorProperty.GetString(context.Properties);
        var phoneNumber = PhoneNumberProperty.GetNumber(context.Properties);
        var phoneNumbers = PhoneNumbersProperty.GetString(context.Properties);

        if (!string.IsNullOrWhiteSpace(originatorName))
        {
            return originatorName;
        }

        if (!string.IsNullOrWhiteSpace(phoneNumbers) && to.Length > 2)
        {
            // Use the country code of the phone number to lookup up the phone number..
            var targetCountryCode = to[..2];

            foreach (var line in phoneNumbers.Split('\n'))
            {
                var number = PhoneNumberHelper.Trim(line);

                if (number.Length > 5)
                {
                    var parts = number.Split(':');

                    if (parts.Length == 2)
                    {
                        var countryCode = parts[0].Trim();

                        if (string.Equals(targetCountryCode, countryCode, StringComparison.OrdinalIgnoreCase))
                        {
                            return parts[1].Trim();
                        }
                    }
                    else if (parts.Length == 1)
                    {
                        var countryCode = number[..2].Trim();

                        if (string.Equals(targetCountryCode, countryCode, StringComparison.OrdinalIgnoreCase))
                        {
                            return number;
                        }
                    }
                }
            }
        }

        return phoneNumber.ToString(CultureInfo.InvariantCulture);
    }

    public async Task HandleRequestAsync(IntegrationContext context, HttpContext httpContext,
        CancellationToken ct)
    {
        var status = await GetClient(context).ParseSmsWebhookAsync(httpContext);

        var result = ParseStatus(status);
        if (result == default)
        {
            return;
        }

        await context.UpdateStatusAsync(status.Reference, result);

        static DeliveryResult ParseStatus(SmsWebhookRequest status)
        {
            switch (status.Status)
            {
                case MessageBirdStatus.Delivered:
                    return DeliveryResult.Handled;
                case MessageBirdStatus.Delivery_Failed:
                    return DeliveryResult.Failed();
                case MessageBirdStatus.Sent:
                    return DeliveryResult.Sent;
                default:
                    return default;
            }
        }
    }

    private IMessageBirdClient GetClient(IntegrationContext context)
    {
        var accessKey = AccessKeyProperty.GetString(context.Properties);

        return clientPool.GetClient(accessKey);
    }
}

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

public sealed class MessageBirdSmsSender : ISmsSender, IIntegrationHook
{
    private readonly ISmsCallback callback;
    private readonly IMessageBirdClient messageBirdClient;
    private readonly string? originatorName;
    private readonly string? originatorNumber;
    private readonly string webhookUrl;
    private readonly Dictionary<string, string>? phoneNumbers;

    public string Name => "MessageBird SMS";

    public MessageBirdSmsSender(
        ISmsCallback callback,
        IMessageBirdClient messageBirdClient,
        string webhookUrl,
        string? originatorName,
        string? originatorNumber,
        Dictionary<string, string>? phoneNumbers)
    {
        this.callback = callback;
        this.messageBirdClient = messageBirdClient;
        this.originatorName = originatorName;
        this.originatorNumber = originatorNumber;
        this.webhookUrl = webhookUrl;
        this.phoneNumbers = phoneNumbers;
    }

    public async Task<DeliveryResult> SendAsync(SmsMessage message,
        CancellationToken ct)
    {
        try
        {
            // Call the phone number and use a local phone number for the user.
            var sms = new Implementation.SmsMessage(
                GetOriginator(message.To),
                message.To,
                message.Text,
                message.NotificationId.ToString(),
                webhookUrl);

            var response = await messageBirdClient.SendSmsAsync(sms, ct);

            // Usually an error is received by the error response in the client, but in some cases it was not working properly.
            if (response.Recipients.TotalSentCount != 1)
            {
                var errorMessage = string.Format(CultureInfo.CurrentCulture, Texts.MessageBird_ErrorUnknown, message.To);

                throw new DomainException(errorMessage);
            }

            // We get the status asynchronously via webhook, therefore we tell the channel not mark the process as completed.
            return DeliveryResult.Sent;
        }
        catch (ArgumentException ex)
        {
            var errorMessage = string.Format(CultureInfo.CurrentCulture, Texts.MessageBird_Error, message.To, ex.Message);

            throw new DomainException(errorMessage);
        }
    }

    private string GetOriginator(string to)
    {
        if (!string.IsNullOrWhiteSpace(originatorName))
        {
            return originatorName;
        }

        if (phoneNumbers?.Count > 0 && to.Length > 2)
        {
            // Use the country code of the phone number to lookup up the phone number..
            var countryCode = to[..2];

            if (phoneNumbers.TryGetValue(countryCode, out var originator))
            {
                return originator;
            }
        }

        return originatorNumber!;
    }

    public async Task HandleRequestAsync(IntegrationContext context, HttpContext httpContext)
    {
        var status = await messageBirdClient.ParseSmsWebhookAsync(httpContext);

        // If the reference is not a valid guid (notification-id), something just went wrong.
        if (!Guid.TryParse(status.Reference, out var notificationId))
        {
            return;
        }

        var result = ParseStatus(status);

        if (result == default)
        {
            return;
        }

        await callback.HandleCallbackAsync(this, notificationId, status.Recipient, result);

        static DeliveryResult ParseStatus(SmsWebhookRequest status)
        {
            switch (status.Status)
            {
                case MessageBirdStatus.Delivered:
                    return DeliveryResult.Delivered;
                case MessageBirdStatus.Delivery_Failed:
                    return DeliveryResult.Failed;
                case MessageBirdStatus.Sent:
                    return DeliveryResult.Sent;
                default:
                    return default;
            }
        }
    }
}

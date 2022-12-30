// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Microsoft.AspNetCore.Http;
using Notifo.Domain.Channels.Sms;
using Notifo.Domain.Integrations.MessageBird.Implementation;
using Notifo.Domain.Integrations.Resources;
using Notifo.Infrastructure;

namespace Notifo.Domain.Integrations.MessageBird;

public sealed class MessageBirdSmsSender : ISmsSender
{
    private readonly IMessageBirdClient messageBirdClient;
    private readonly string? originatorName;
    private readonly string? originatorNumber;
    private readonly Dictionary<string, string>? phoneNumbers;

    public string Name => "MessageBird SMS";

    public MessageBirdSmsSender(
        IMessageBirdClient messageBirdClient,
        string? originatorName,
        string? originatorNumber,
        Dictionary<string, string>? phoneNumbers)
    {
        this.messageBirdClient = messageBirdClient;
        this.originatorName = originatorName;
        this.originatorNumber = originatorNumber;
        this.phoneNumbers = phoneNumbers;
    }

    public async Task<SmsResult> SendAsync(SmsRequest request,
        CancellationToken ct = default)
    {
        var (to, message, callbackUrl) = request;
        try
        {
            // Call the phone number and use a local phone number for the user.
            var sms = new SmsMessage(GetOriginator(to), to, message, null, callbackUrl);

            var response = await messageBirdClient.SendSmsAsync(sms, ct);

            // Usually an error is received by the error response in the client, but in some cases it was not working properly.
            if (response.Recipients.TotalSentCount != 1)
            {
                var errorMessage = string.Format(CultureInfo.CurrentCulture, Texts.MessageBird_ErrorUnknown, request.To);

                throw new DomainException(errorMessage);
            }

            // We get the status asynchronously via webhook, therefore we tell the channel not mark the process as completed.
            return SmsResult.Sent;
        }
        catch (ArgumentException ex)
        {
            var errorMessage = string.Format(CultureInfo.CurrentCulture, Texts.MessageBird_Error, to, ex.Message);

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

    public async ValueTask<SmsCallbackResponse> HandleCallbackAsync(HttpContext httpContext)
    {
        var status = await messageBirdClient.ParseSmsWebhookAsync(httpContext);

        return new SmsCallbackResponse(ParseStatus(status));

        static SmsResult ParseStatus(SmsWebhookRequest status)
        {
            switch (status.Status)
            {
                case MessageBirdStatus.Delivered:
                    return SmsResult.Delivered;
                case MessageBirdStatus.Delivery_Failed:
                    return SmsResult.Failed;
                case MessageBirdStatus.Sent:
                    return SmsResult.Sent;
                default:
                    return default;
            }
        }
    }
}

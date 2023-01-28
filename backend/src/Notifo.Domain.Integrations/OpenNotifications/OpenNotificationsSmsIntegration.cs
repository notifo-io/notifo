// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http;
using Notifo.Infrastructure;
using OpenNotifications;

namespace Notifo.Domain.Integrations.OpenNotifications;

public sealed class OpenNotificationsSmsIntegration : OpenNotificationsIntegrationBase, ISmsSender, IIntegrationHook
{
    private readonly ISmsCallback smsCallback;

    public OpenNotificationsSmsIntegration(string fullName, string providerName, ProviderInfoDto providerInfo, IOpenNotificationsClient client,
        ISmsCallback smsCallback)
        : base(fullName, providerName, providerInfo, client)
    {
        this.smsCallback = smsCallback;
    }

    public async Task<DeliveryResult> SendAsync(IntegrationContext context, SmsMessage message, CancellationToken ct)
    {
        var requestDto = new SendSmsRequestDto
        {
            Context = context.ToContext(),
            Properties = context.Properties.ToProperties(Definition),
            Provider = ProviderName,
            Payload = new SmsPayloadDto
            {
                To = message.To,
                Body = message.Text,
            }
        };

        var status = await Client.Providers.SendSmsAsync(requestDto, ct);

        return status.Status.ToDeliveryStatus();
    }

    public async Task HandleRequestAsync(IntegrationContext context, HttpContext httpContext,
        CancellationToken ct)
    {
        var httpRequest = httpContext.Request;

        var requestDto = new WebhookRequestDto
        {
            Context = context.ToContext(),
            Properties = context.Properties.ToProperties(Definition),
            Provider = ProviderName,
            Query = httpRequest.Query.ToDictionary(
                x => x.Key,
                x => x.Value.NotNull().ToList()),
            Headers = httpRequest.Headers.ToDictionary(
                x => x.Key,
                x => x.Value.ToString()),
            Body = await new StreamReader(httpRequest.Body).ReadLineAsync(default),
        };

        var response = await Client.Providers.HandleWebhookAsync(requestDto, default);

        var http = response.Http;

        if (http != null)
        {
            var httpResponse = httpContext.Response;

            if (http.Headers != null)
            {
                foreach (var (key, value) in http.Headers)
                {
                    httpResponse.Headers[key] = value;
                }
            }

            if (http.StatusCode != null)
            {
                httpResponse.StatusCode = http.StatusCode.Value;
            }

            if (http.Body != null)
            {
                await httpResponse.WriteAsync(http.Body, default);
            }
        }

        var status = response.Status;

        if (status != null)
        {
            await smsCallback.HandleCallbackAsync(this,
                status.NotificationId,
                status.Status.ToDeliveryStatus(),
                status.Errors?.FirstOrDefault()?.Message);
        }
    }
}

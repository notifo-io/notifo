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
    public OpenNotificationsSmsIntegration(string fullName, string providerName, ProviderInfoDto providerInfo, IOpenNotificationsClient client)
        : base(fullName, providerName, providerInfo, client)
    {
    }

    public async Task<DeliveryResult> SendAsync(IntegrationContext context, SmsMessage message, CancellationToken ct)
    {
        try
        {
            var requestDto = new SendSmsRequestDto
            {
                Context = context.ToContext(),
                Payload = new SmsPayloadDto
                {
                    Body = message.Text,
                    // We only support one phone number at the moment.
                    To = message.To,
                },
                Properties = context.Properties.ToProperties(Definition),
                Provider = ProviderName,
                TrackingToken = message.TrackingToken,
                TrackingWebhookUrl = message.TrackSeenUrl ?? "none",
            };

            var status = await Client.Providers.SendSmsAsync(requestDto, ct);

            return status.ToDeliveryResult();
        }
        catch (OpenNotificationsException ex)
        {
            throw ex.ToDomainException();
        }
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
            await context.UpdateStatusAsync(status.TrackingToken, status.ToDeliveryResult());
        }
    }
}

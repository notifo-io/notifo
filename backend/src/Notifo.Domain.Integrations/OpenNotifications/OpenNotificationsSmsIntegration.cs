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

        if (response.Http != null)
        {
            var httpResponse = httpContext.Response;

            if (response.Http.Headers != null)
            {
                foreach (var (key, value) in response.Http.Headers)
                {
                    httpResponse.Headers[key] = value;
                }
            }

            if (response.Http.StatusCode != null)
            {
                httpResponse.StatusCode = response.Http.StatusCode.Value;
            }

            if (response.Http.Body != null)
            {
                await httpResponse.WriteAsync(response.Http.Body, default);
            }
        }

        if (response.Statuses != null)
        {
            foreach (var status in response.Statuses)
            {
                await context.UpdateStatusAsync(status.TrackingToken, status.ToDeliveryResult());
            }
        }
    }
}

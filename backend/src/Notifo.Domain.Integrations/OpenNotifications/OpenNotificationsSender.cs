// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Notifo.Infrastructure;
using OpenNotifications;

namespace Notifo.Domain.Integrations.OpenNotifications;

public sealed class OpenNotificationsSender : IEmailSender, ISmsSender, IIntegrationHook
{
    private readonly ProviderInfoDtoType type;
    private readonly IntegrationContext context;
    private readonly IntegrationDefinition definition;
    private readonly string providerName;
    private readonly IOpenNotificationsClient client;
    private readonly IServiceProvider services;

    public string Name => definition.Type;

    public OpenNotificationsSender(
        ProviderInfoDtoType type,
        IntegrationContext context,
        IntegrationDefinition definition,
        string providerName,
        IOpenNotificationsClient client,
        IServiceProvider services)
    {
        this.type = type;
        this.context = context;
        this.definition = definition;
        this.providerName = providerName;
        this.client = client;
        this.services = services;
    }

    public async Task SendAsync(EmailMessage request,
        CancellationToken ct = default)
    {
        var requestDto = new SendEmailRequestDto
        {
            Properties = context.Properties.ToProperties(definition),
            Provider = providerName,
            Payload = new EmailPayloadDto
            {
                BodyHtml = request.BodyHtml,
                BodyText = request.BodyText,
                FromEmail = request.FromEmail,
                FromName = request.FromName!,
                Subject = request.Subject,
                To = request.ToEmail
            },
            Context = context.ToContext(),
        };

        await client.Providers.SendEmailAsync(requestDto, ct);
    }

    public async Task<DeliveryResult> SendAsync(SmsMessage message, CancellationToken ct)
    {
        var requestDto = new SendSmsRequestDto
        {
            Properties = context.Properties.ToProperties(definition),
            Provider = providerName,
            Payload = new SmsPayloadDto
            {
                To = message.To,
                Body = message.Text,
            },
            Context = context.ToContext(),
        };

        var status = await client.Providers.SendSmsAsync(requestDto, ct);

        return status.Status.ToDeliveryStatus();
    }

    public async Task HandleRequestAsync(HttpContext httpContext)
    {
        var httpRequest = httpContext.Request;

        var requestDto = new WebhookRequestDto
        {
            Properties = context.Properties.ToProperties(definition),
            Provider = providerName,
            Context = context.ToContext(),
            Query = httpRequest.Query.ToDictionary(
                x => x.Key,
                x => x.Value.NotNull().ToList()),
            Headers = httpRequest.Headers.ToDictionary(
                x => x.Key,
                x => x.Value.ToString()),
            Body = await new StreamReader(httpRequest.Body).ReadLineAsync(default),
        };

        var response = await client.Providers.HandleWebhookAsync(requestDto, default);

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
            if (type == ProviderInfoDtoType.Sms)
            {
                var smsCallback = services.GetRequiredService<ISmsCallback>();

                await smsCallback.HandleCallbackAsync(this,
                    status.NotificationId,
                    status.Status.ToDeliveryStatus(),
                    status.Errors?.FirstOrDefault()?.Message);
            }
        }
    }
}

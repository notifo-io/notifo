// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net;
using Microsoft.AspNetCore.Http;
using Notifo.Domain.Integrations.MessageBird.Implementation;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Tasks;

namespace Notifo.Domain.Integrations.MessageBird;

public sealed partial class MessageBirdWhatsAppIntegration : IMessagingSender, IIntegrationHook
{
    private const string WhatsAppPhoneNumber = nameof(WhatsAppPhoneNumber);

    public void AddTargets(IDictionary<string, string> targets, UserInfo user)
    {
        var phoneNumber = user.PhoneNumber;

        if (!string.IsNullOrWhiteSpace(phoneNumber))
        {
            targets[WhatsAppPhoneNumber] = phoneNumber;
        }
    }

    public async Task<DeliveryResult> SendAsync(IntegrationContext context, MessagingMessage message, IReadOnlyDictionary<string, string> targets,
        CancellationToken ct)
    {
        if (!targets.TryGetValue(WhatsAppPhoneNumber, out var to))
        {
            return DeliveryResult.Skipped;
        }

        var channelId = WhatsAppChannelIdProperty.GetString(context.Properties);
        var templateNamespace = WhatsAppTemplateNamespaceProperty.GetString(context.Properties);
        var templateName = WhatsAppTemplateNameProperty.GetString(context.Properties);

        var textMessage = new WhatsAppTemplateMessage(
            channelId,
            to,
            templateNamespace,
            templateName,
            message.UserLanguage,
            context.WebhookUrl.AppendQueries("reference", message.TrackingToken),
            new[] { message.Text });

        var client = GetClient(context);

        // Just send the normal text message.
        var response = await GetClient(context).SendWhatsAppAsync(textMessage, ct);

        // Query for the status, otherwise we cannot retrieve errors.
        QueryAsync(client, message, response).Forget();

        return DeliveryResult.Sent;
    }

    private async Task QueryAsync(IMessageBirdClient client, MessagingMessage message, ConversationResponse response)
    {
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        while (response.Status is MessageBirdStatus.Pending or MessageBirdStatus.Accepted && !cts.IsCancellationRequested)
        {
            try
            {
                response = await client.GetMessageAsync(response.Id, cts.Token);

                if (response.Status == MessageBirdStatus.Pending)
                {
                    await Task.Delay(200, cts.Token);
                    continue;
                }

                var result = default(DeliveryResult);

                switch (response.Status)
                {
                    case MessageBirdStatus.Delivered:
                        result = DeliveryResult.Delivered;
                        break;
                    case MessageBirdStatus.Delivery_Failed:
                    case MessageBirdStatus.Failed:
                    case MessageBirdStatus.Rejected:
                        result = DeliveryResult.Failed;
                        break;
                    case MessageBirdStatus.Sent:
                        result = DeliveryResult.Sent;
                        break;
                }

                if (result != default)
                {
                    await callback.HandleCallbackAsync(this, message.TrackingToken, result);
                    return;
                }
            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode != HttpStatusCode.NotFound)
                {
                    break;
                }
            }
        }
    }

    public async Task HandleRequestAsync(IntegrationContext context, HttpContext httpContext,
        CancellationToken ct)
    {
        var status = await GetClient(context).ParseWhatsAppWebhookAsync(httpContext);

        if (!status.Query.TryGetValue("reference", out var reference))
        {
            return;
        }

        var deliveryStatus = ParseStatus(status);

        if (deliveryStatus == default)
        {
            return;
        }

        await callback.HandleCallbackAsync(this, reference, deliveryStatus, status.Error?.Description);

        static DeliveryResult ParseStatus(WhatsAppWebhookRequest status)
        {
            switch (status.Message.Status)
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

    private IMessageBirdClient GetClient(IntegrationContext context)
    {
        var accessKey = AccessKeyProperty.GetString(context.Properties);

        return clientPool.GetClient(accessKey);
    }
}

// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net;
using Microsoft.AspNetCore.Http;
using Notifo.Domain.Channels.Messaging;
using Notifo.Domain.Integrations.MessageBird.Implementation;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Tasks;

namespace Notifo.Domain.Integrations.MessageBird;

public sealed class MessageBirdWhatsAppSender : IMessagingSender, IIntegrationHook
{
    private const string WhatsAppPhoneNumber = nameof(WhatsAppPhoneNumber);
    private readonly IntegrationContext context;
    private readonly IMessagingCallback callback;
    private readonly IMessageBirdClient messageBirdClient;
    private readonly string channelId;
    private readonly string templateNamespace;
    private readonly string templateName;

    public string Name => "Messagbird Whatsapp";

    public MessageBirdWhatsAppSender(
        IntegrationContext context,
        IMessagingCallback callback,
        IMessageBirdClient messageBirdClient,
        string channelId,
        string templateNamespace,
        string templateName)
    {
        this.context = context;
        this.callback = callback;
        this.messageBirdClient = messageBirdClient;
        this.channelId = channelId;
        this.templateNamespace = templateNamespace;
        this.templateName = templateName;
    }

    public void AddTargets(IDictionary<string, string> targets, UserContext user)
    {
        var phoneNumber = user.PhoneNumber;

        if (!string.IsNullOrWhiteSpace(phoneNumber))
        {
            targets[WhatsAppPhoneNumber] = phoneNumber;
        }
    }

    public async Task<DeliveryResult> SendAsync(MessagingMessage message, IReadOnlyDictionary<string, string> targets,
        CancellationToken ct)
    {
        if (!targets.TryGetValue(WhatsAppPhoneNumber, out var to))
        {
            return DeliveryResult.Skipped;
        }

        var textMessage = new WhatsAppTemplateMessage(
            channelId,
            to,
            templateNamespace,
            templateName,
            message.UserLanguage,
            context.WebhookUrl.AppendQueries("reference", message.TrackingToken),
            new[] { message.Text });

        // Just send the normal text message.
        var response = await messageBirdClient.SendWhatsAppAsync(textMessage, ct);

        // Query for the status, otherwise we cannot retrieve errors.
        QueryAsync(message, response).Forget();

        return DeliveryResult.Sent;
    }

    private async Task QueryAsync(MessagingMessage message, ConversationResponse response)
    {
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        while (response.Status is MessageBirdStatus.Pending or MessageBirdStatus.Accepted && !cts.IsCancellationRequested)
        {
            try
            {
                response = await messageBirdClient.GetMessageAsync(response.Id, cts.Token);

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

    public async Task HandleRequestAsync(HttpContext httpContext)
    {
        var status = await messageBirdClient.ParseWhatsAppWebhookAsync(httpContext);

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
}

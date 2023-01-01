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
    private readonly IMessagingCallback callback;
    private readonly IMessageBirdClient messageBirdClient;
    private readonly string channelId;
    private readonly string templateNamespace;
    private readonly string templateName;

    public string Name => "Messagbird Whatsapp";

    public MessageBirdWhatsAppSender(
        IMessagingCallback callback,
        IMessageBirdClient messageBirdClient,
        string channelId,
        string templateNamespace,
        string templateName)
    {
        this.callback = callback;
        this.messageBirdClient = messageBirdClient;
        this.channelId = channelId;
        this.templateNamespace = templateNamespace;
        this.templateName = templateName;
    }

    public void AddTargets(MessagingTargets targets, UserContext user)
    {
        var phoneNumber = user.PhoneNumber;

        if (!string.IsNullOrWhiteSpace(phoneNumber))
        {
            targets[WhatsAppPhoneNumber] = phoneNumber;
        }
    }

    public async Task<MessagingResult> SendAsync(MessagingMessage message,
        CancellationToken ct)
    {
        if (!message.Targets.TryGetValue(WhatsAppPhoneNumber, out var to))
        {
            return MessagingResult.Skipped;
        }

        var textMessage = new WhatsAppTemplateMessage(
            channelId,
            to,
            templateNamespace,
            templateName,
            message.Language,
            message.ReportUrl.AppendQueries("reference", message.NotificationId),
            new[] { message.Text });

        // Just send the normal text message.
        var response = await messageBirdClient.SendWhatsAppAsync(textMessage, ct);

        // Query for the status, otherwise we cannot retrieve errors.
        QueryAsync(message, response).Forget();

        return MessagingResult.Sent;
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

                var result = default(MessagingResult);

                switch (response.Status)
                {
                    case MessageBirdStatus.Delivered:
                        result = MessagingResult.Delivered;
                        break;
                    case MessageBirdStatus.Delivery_Failed:
                    case MessageBirdStatus.Failed:
                    case MessageBirdStatus.Rejected:
                        result = MessagingResult.Failed;
                        break;
                    case MessageBirdStatus.Sent:
                        result = MessagingResult.Sent;
                        break;
                }

                if (result != default)
                {
                    await callback.HandleCallbackAsync(this, message.NotificationId, result);
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

    public async Task HandleRequestAsync(AppContext app, HttpContext httpContext)
    {
        var status = await messageBirdClient.ParseWhatsAppWebhookAsync(httpContext);

        // If the reference is not a valid guid (notification-id), something just went wrong.
        if (!status.Query.TryGetValue("reference", out var query) || !Guid.TryParse(query, out var notificationId))
        {
            return;
        }

        var result = ParseStatus(status);

        if (result == default)
        {
            return;
        }

        await callback.HandleCallbackAsync(this, notificationId, result, status.Error?.Description);

        static MessagingResult ParseStatus(WhatsAppWebhookRequest status)
        {
            switch (status.Message.Status)
            {
                case MessageBirdStatus.Delivered:
                    return MessagingResult.Delivered;
                case MessageBirdStatus.Delivery_Failed:
                    return MessagingResult.Failed;
                case MessageBirdStatus.Sent:
                    return MessagingResult.Sent;
                default:
                    return default;
            }
        }
    }
}

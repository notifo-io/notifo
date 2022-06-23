// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http;
using Notifo.Domain.Apps;
using Notifo.Domain.Channels.Messaging;
using Notifo.Domain.Integrations.MessageBird.Implementation;
using Notifo.Domain.Users;
using Notifo.Infrastructure.Tasks;
using System.Net;

namespace Notifo.Domain.Integrations.MessageBird
{
    public sealed class MessageBirdWhatsAppSender : IMessagingSender
    {
        private const string WhatsAppPhoneNumber = nameof(WhatsAppPhoneNumber);
        private readonly IMessageBirdClient messageBirdClient;
        private readonly IMessagingCallback messagingCallback;
        private readonly IMessagingUrl messagingUrl;
        private readonly IUserStore userStore;
        private readonly string integrationId;
        private readonly string channelId;
        private readonly string templateNamespace;
        private readonly string templateName;

        public string Name => "MessageBird WhatsApp";

        public MessageBirdWhatsAppSender(
            IMessageBirdClient messageBirdClient,
            IMessagingCallback messagingCallback,
            IMessagingUrl messagingUrl,
            IUserStore userStore,
            string integrationId,
            string channelId,
            string templateNamespace,
            string templateName)
        {
            this.messageBirdClient = messageBirdClient;
            this.messagingCallback = messagingCallback;
            this.messagingUrl = messagingUrl;
            this.userStore = userStore;
            this.integrationId = integrationId;
            this.channelId = channelId;
            this.templateNamespace = templateNamespace;
            this.templateName = templateName;
        }

        public bool HasTarget(User user)
        {
            return !string.IsNullOrWhiteSpace(user.PhoneNumber);
        }

        public Task AddTargetsAsync(MessagingJob job, User user)
        {
            var phoneNumber = user.PhoneNumber;

            if (!string.IsNullOrWhiteSpace(phoneNumber))
            {
                job.Targets[WhatsAppPhoneNumber] = phoneNumber;
            }

            return Task.CompletedTask;
        }

        public async Task<MessagingResult> SendAsync(MessagingJob job, string text,
            CancellationToken ct)
        {
            var user = await userStore.GetAsync(job.Notification.AppId, job.Notification.UserId, ct);

            if (user == null)
            {
                return MessagingResult.Skipped;
            }

            var to = job.Targets[WhatsAppPhoneNumber];

            var queries = new Dictionary<string, string>
            {
                ["reference"] = job.Notification.Id.ToString()
            };

            var reportUrl = messagingUrl.MessagingWebhookUrl(job.Notification.AppId, integrationId, queries);

            var textMessage = new WhatsAppTemplateMessage(
                channelId,
                to,
                templateNamespace,
                templateName,
                user.PreferredLanguage,
                reportUrl,
                new[] { text });

            // Just send the normal text message.
            var response = await messageBirdClient.SendWhatsAppAsync(textMessage, ct);

            // Query for the status, otherwise we cannot retrieve errors.
            QueryAsync(job.Notification.Id, to, response).Forget();

            // We get the status asynchronously via webhook, therefore we tell the channel not mark the process as completed.
            return MessagingResult.Sent;
        }

        private async Task QueryAsync(Guid id, string to, ConversationResponse response)
        {
            using (var tcs = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            {
                while ((response.Status == MessageBirdStatus.Pending || response.Status == MessageBirdStatus.Accepted) && !tcs.IsCancellationRequested)
                {
                    try
                    {
                        response = await messageBirdClient.GetMessageAsync(response.Id, tcs.Token);

                        if (response.Status == MessageBirdStatus.Pending)
                        {
                            await Task.Delay(200, tcs.Token);
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

                        if (result == MessagingResult.Unknown)
                        {
                            continue;
                        }

                        var callback = new MessagingCallbackResponse(id, result, response.Error?.Description);

                        await messagingCallback.HandleCallbackAsync(this, callback, tcs.Token);
                    }
                    catch (HttpRequestException ex)
                    {
                        if (ex.StatusCode != HttpStatusCode.NotFound)
                        {
                            return;
                        }
                    }
                }
            }
        }

        public async Task HandleCallbackAsync(App app, HttpContext httpContext)
        {
            var status = await messageBirdClient.ParseWhatsAppWebhookAsync(httpContext);

            // If the reference is not a valid guid (notification-id), something just went wrong.
            if (!status.Query.TryGetValue("reference", out var query) || !Guid.TryParse(query, out var reference) || reference == default)
            {
                return;
            }

            if (status.Message.Status == MessageBirdStatus.Pending)
            {
                return;
            }

            var result = default(MessagingResult);

            switch (status.Message.Status)
            {
                case MessageBirdStatus.Delivered:
                    result = MessagingResult.Delivered;
                    break;
                case MessageBirdStatus.Delivery_Failed:
                    result = MessagingResult.Failed;
                    break;
                case MessageBirdStatus.Sent:
                    result = MessagingResult.Sent;
                    break;
            }

            if (result == MessagingResult.Unknown)
            {
                return;
            }

            var callback = new MessagingCallbackResponse(reference, result, status.Error?.Description);

            await messagingCallback.HandleCallbackAsync(this, callback, httpContext.RequestAborted);
        }
    }
}

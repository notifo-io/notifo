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

            var sentKey = $"MessageBird_WhatsApp_{channelId}_{templateName}";

            // We need to send an initial template message to talk with the user.
            if (user.SystemProperties?.ContainsKey(sentKey) != true)
            {
                var templateMesage = new WhatsAppTemplateMessage(
                    channelId,
                    to,
                    templateNamespace,
                    templateName,
                    user.PreferredLanguage);

                await messageBirdClient.SendWhatsAppAsync(templateMesage, ct);

                await userStore.UpsertAsync(job.Notification.AppId, job.Notification.UserId, new SetUserSystemProperty
                {
                    PropertyKey = sentKey,
                    PropertyValue = "true",
                }, ct);
            }

            var textMessageUrl = messagingUrl.MessagingWebhookUrl(job.Notification.AppId, integrationId);
            var textMessage = new WhatsAppTextMessage(channelId, to, text, job.Notification.Id.ToString(), textMessageUrl);

            // Just send the normal text message.
            await messageBirdClient.SendWhatsAppAsync(textMessage, ct);

            return MessagingResult.Sent;
        }

        public async Task HandleCallbackAsync(App app, HttpContext httpContext)
        {
            var status = await messageBirdClient.ParseWhatsAppStatusAsync(httpContext);

            // If the reference is not a valid guid (notification-id), something just went wrong.
            if (status.Reference == default)
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

            await messagingCallback.HandleCallbackAsync(status.To, status.Reference, result, httpContext.RequestAborted);
        }
    }
}

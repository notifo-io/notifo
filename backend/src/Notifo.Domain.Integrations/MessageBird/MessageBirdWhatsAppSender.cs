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
        private readonly IUserStore userStore;
        private readonly string channelId;
        private readonly string templateNamespace;
        private readonly string templateName;

        public MessageBirdWhatsAppSender(
            IMessageBirdClient messageBirdClient,
            IUserStore userStore,
            string channelId,
            string templateNamespace,
            string templateName)
        {
            this.messageBirdClient = messageBirdClient;
            this.userStore = userStore;
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

            var textMessage = new WhatsAppTextMessage(channelId, to, text);

            // Just send the normal text message.
            await messageBirdClient.SendWhatsAppAsync(textMessage, ct);

            return MessagingResult.Delivered;
        }

        public Task HandleCallbackAsync(App app, HttpContext httpContext)
        {
            return Task.CompletedTask;
        }
    }
}

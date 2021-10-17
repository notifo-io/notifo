// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Notifo.Domain.Apps;
using Notifo.Domain.Channels;
using Notifo.Domain.Channels.Messaging;
using Notifo.Domain.Integrations.Resources;
using Notifo.Domain.Users;
using Telegram.Bot;

namespace Notifo.Domain.Integrations.Telegram
{
    public sealed class TelegramIntegration : IIntegration
    {
        private readonly TelegramBotClientPool botClientPool;

        private static readonly IntegrationProperty AccessToken = new IntegrationProperty("accessToken", IntegrationPropertyType.Text)
        {
            EditorLabel = Texts.Telegram_AccessKeyLabel,
            EditorDescription = null,
            IsRequired = true,
            Summary = true
        };
        private readonly IMessagingUrl messagingUrl;
        private readonly IUserStore userStore;

        public IntegrationDefinition Definition { get; } =
            new IntegrationDefinition(
                "Telegram",
                Texts.Telegram_Name,
                "./integrations/telegram.svg",
                new List<IntegrationProperty>
                {
                    AccessToken
                },
                new HashSet<string>
                {
                    Providers.Messaging
                })
            {
                Description = Texts.Telegram_Description
            };

        public TelegramIntegration(TelegramBotClientPool botClientPool, IMessagingUrl messagingUrl, IUserStore userStore)
        {
            this.botClientPool = botClientPool;
            this.messagingUrl = messagingUrl;
            this.userStore = userStore;
        }

        public bool CanCreate(Type serviceType, string id, ConfiguredIntegration configured)
        {
            return serviceType == typeof(IMessagingSender);
        }

        public object? Create(Type serviceType, string id, ConfiguredIntegration configured)
        {
            if (CanCreate(serviceType, id, configured))
            {
                var accessToken = AccessToken.GetString(configured);

                if (string.IsNullOrWhiteSpace(accessToken))
                {
                    return null;
                }

                return new TelegramMessagingSender(() => botClientPool.GetBotClient(accessToken), userStore);
            }

            return null;
        }

        public async Task OnConfiguredAsync(App app, string id, ConfiguredIntegration configured, ConfiguredIntegration? previous,
            CancellationToken ct)
        {
            var client = GetBotClient(configured);

            var url = messagingUrl.MessagingWebhookUrl(app.Id, id);

            await client.SetWebhookAsync(url, cancellationToken: ct);
        }

        public async Task OnRemovedAsync(App app, string id, ConfiguredIntegration configured,
            CancellationToken ct)
        {
            var client = GetBotClient(configured);

            await client.DeleteWebhookAsync(cancellationToken: ct);
        }

        private ITelegramBotClient GetBotClient(ConfiguredIntegration configured)
        {
            var accessToken = AccessToken.GetString(configured);

            return botClientPool.GetBotClient(accessToken!);
        }
    }
}

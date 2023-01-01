// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Notifo.Domain.Channels.Messaging;
using Notifo.Domain.Integrations.Resources;
using Telegram.Bot;

namespace Notifo.Domain.Integrations.Telegram;

public sealed class TelegramIntegration : IIntegration
{
    private readonly TelegramBotClientPool botClientPool;
    private readonly IMessagingUrl messagingUrl;

    public static readonly UserProperty UserUsername = new UserProperty("telegramUserId", PropertyType.Text)
    {
        EditorLabel = Texts.Telegram_UsernameLabel,
        EditorDescription = Texts.Telegram_UsernameDescription
    };

    public static readonly UserProperty UserChatId = new UserProperty("telegramChatId", PropertyType.Text)
    {
        EditorLabel = Texts.Telegram_ChatIdLabel,
        EditorDescription = Texts.Telegram_ChatIdDescription
    };

    private static readonly IntegrationProperty AccessToken = new IntegrationProperty("accessToken", PropertyType.Text)
    {
        EditorLabel = Texts.Telegram_AccessKeyLabel,
        EditorDescription = null,
        IsRequired = true
    };

    public IntegrationDefinition Definition { get; } =
        new IntegrationDefinition(
            "Telegram",
            Texts.Telegram_Name,
            "./integrations/telegram.svg",
            new List<IntegrationProperty>
            {
                AccessToken
            },
            new List<UserProperty>
            {
                UserUsername,
                UserChatId
            },
            new HashSet<string>
            {
                Providers.Messaging
            })
        {
            Description = Texts.Telegram_Description
        };

    public TelegramIntegration(TelegramBotClientPool botClientPool, IMessagingUrl messagingUrl)
    {
        this.botClientPool = botClientPool;
        this.messagingUrl = messagingUrl;
    }

    public bool CanCreate(Type serviceType, string id, IntegrationConfiguration configured)
    {
        return serviceType == typeof(IMessagingSender);
    }

    public object? Create(Type serviceType, string id, IntegrationConfiguration configured, IServiceProvider serviceProvider)
    {
        if (CanCreate(serviceType, id, configured))
        {
            var accessToken = AccessToken.GetString(configured);

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return null;
            }

            return new TelegramMessagingSender(
                serviceProvider.GetRequiredService<IIntegrationAdapter>(),
                serviceProvider.GetRequiredService<IMessagingCallback>(),
                () => botClientPool.GetBotClient(accessToken));
        }

        return null;
    }

    public async Task OnConfiguredAsync(AppContext app, string id, IntegrationConfiguration configured, IntegrationConfiguration? previous,
        CancellationToken ct)
    {
        var client = GetBotClient(configured);

        var url = messagingUrl.MessagingWebhookUrl(app.Id, id);

        await client.SetWebhookAsync(url, cancellationToken: ct);
    }

    public async Task OnRemovedAsync(AppContext app, string id, IntegrationConfiguration configured,
        CancellationToken ct)
    {
        var client = GetBotClient(configured);

        await client.DeleteWebhookAsync(cancellationToken: ct);
    }

    private ITelegramBotClient GetBotClient(IntegrationConfiguration configured)
    {
        var accessToken = AccessToken.GetString(configured);

        return botClientPool.GetBotClient(accessToken!);
    }
}

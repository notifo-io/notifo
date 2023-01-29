// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Integrations.Resources;
using Telegram.Bot;

namespace Notifo.Domain.Integrations.Telegram;

public sealed partial class TelegramIntegration : IIntegration
{
    private readonly TelegramBotClientPool clientPool;

    public static readonly IntegrationProperty UserUsername = new IntegrationProperty("telegramUserId", PropertyType.Text)
    {
        EditorLabel = Texts.Telegram_UsernameLabel,
        EditorDescription = Texts.Telegram_UsernameDescription
    };

    public static readonly IntegrationProperty UserChatId = new IntegrationProperty("telegramChatId", PropertyType.Text)
    {
        EditorLabel = Texts.Telegram_ChatIdLabel,
        EditorDescription = Texts.Telegram_ChatIdDescription
    };

    public static readonly IntegrationProperty AccessToken = new IntegrationProperty("accessToken", PropertyType.Text)
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
            new List<IntegrationProperty>
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

    public TelegramIntegration(TelegramBotClientPool clientPool)
    {
        this.clientPool = clientPool;
    }

    public async Task<IntegrationStatus> OnConfiguredAsync(IntegrationContext context, IntegrationConfiguration? previous,
        CancellationToken ct)
    {
        await GetBotClient(context).SetWebhookAsync(context.WebhookUrl, cancellationToken: ct);

        return IntegrationStatus.Verified;
    }

    public async Task OnRemovedAsync(IntegrationContext context,
        CancellationToken ct)
    {
        await GetBotClient(context).DeleteWebhookAsync(cancellationToken: ct);
    }

    private ITelegramBotClient GetBotClient(IntegrationContext context)
    {
        var accessToken = AccessToken.GetString(context.Properties);

        return clientPool.GetBotClient(accessToken!);
    }
}

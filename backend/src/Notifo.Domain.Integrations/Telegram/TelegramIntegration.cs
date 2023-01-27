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

    public TelegramIntegration(TelegramBotClientPool botClientPool)
    {
        this.botClientPool = botClientPool;
    }

    public bool CanCreate(Type serviceType, IntegrationContext context)
    {
        return serviceType == typeof(IMessagingSender);
    }

    public object? Create(Type serviceType, IntegrationContext context, IServiceProvider serviceProvider)
    {
        if (CanCreate(serviceType, context))
        {
            var accessToken = AccessToken.GetString(context.Properties);

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

        return botClientPool.GetBotClient(accessToken!);
    }
}

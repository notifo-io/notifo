// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Integrations.Resources;
using Notifo.Infrastructure.Validation;
using Telegram.Bot;

namespace Notifo.Domain.Integrations.Telegram;

public sealed partial class TelegramIntegration(TelegramBotClientPool clientPool) : IIntegration
{
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
            "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 240 240'><defs><linearGradient id='a' x1='120' y1='240' x2='120' gradientUnits='userSpaceOnUse'><stop offset='0' stop-color='#1d93d2'/><stop offset='1' stop-color='#38b0e3'/></linearGradient></defs><circle cx='120' cy='120' r='120' fill='url(#a)'/><path d='m81.229 128.772 14.237 39.406s1.78 3.687 3.686 3.687 30.255-29.492 30.255-29.492l31.525-60.89L81.737 118.6Z' fill='#c8daea'/><path d='m100.106 138.878-2.733 29.046s-1.144 8.9 7.754 0 17.415-15.763 17.415-15.763' fill='#a9c6d8'/><path d='M81.486 130.178 52.2 120.636s-3.5-1.42-2.373-4.64c.232-.664.7-1.229 2.1-2.2 6.489-4.523 120.106-45.36 120.106-45.36s3.208-1.081 5.1-.362a2.766 2.766 0 0 1 1.885 2.055 9.357 9.357 0 0 1 .254 2.585c-.009.752-.1 1.449-.169 2.542-.692 11.165-21.4 94.493-21.4 94.493s-1.239 4.876-5.678 5.043a8.13 8.13 0 0 1-5.925-2.292c-8.711-7.493-38.819-27.727-45.472-32.177a1.27 1.27 0 0 1-.546-.9c-.093-.469.417-1.05.417-1.05s52.426-46.6 53.821-51.492c.108-.379-.3-.566-.848-.4-3.482 1.281-63.844 39.4-70.506 43.607a3.21 3.21 0 0 1-1.48.09Z' fill='#fff'/></svg>",
            [
                AccessToken
            ],
            [
                UserUsername,
                UserChatId
            ],
            new HashSet<string>
            {
                Providers.Messaging
            })
        {
            Description = Texts.Telegram_Description
        };

    public async Task<IntegrationStatus> OnConfiguredAsync(IntegrationContext context, IntegrationConfiguration? previous,
        CancellationToken ct)
    {
        try
        {
            await GetBotClient(context).SetWebhookAsync(context.WebhookUrl, cancellationToken: ct);
        }
        catch (Exception ex)
        {
            throw new ValidationException($"Failed to configure telegram: {ex.Message}.");
        }

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

// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Discord;
using Discord.Net;

namespace Notifo.Domain.Integrations.Discord;

public sealed partial class DiscordIntegration : IMessagingSender
{
    private const int Attempts = 5;
    public const string DiscordChatId = nameof(DiscordChatId);

    public void AddTargets(IDictionary<string, string> targets, UserInfo user)
    {
        var userId = GetUserId(user);

        if (!string.IsNullOrWhiteSpace(userId))
        {
            targets[DiscordChatId] = userId;
        }
    }

    public async Task<DeliveryResult> SendAsync(IntegrationContext context, MessagingMessage message,
       CancellationToken ct)
    {
        if (!message.Targets.TryGetValue(DiscordChatId, out var chatId))
        {
            return DeliveryResult.Skipped();
        }

        return await SendMessageAsync(context, message, chatId, ct);
    }

    private async Task<DeliveryResult> SendMessageAsync(IntegrationContext context, MessagingMessage message, string chatId,
       CancellationToken ct)
    {
        var botToken = BotToken.GetString(context.Properties);

        for (var i = 1; i <= Attempts; i++)
        {
            try
            {
                var client = await discordBotClientPool.GetDiscordClient(botToken);

                var requestOptions = new RequestOptions
                {
                    CancelToken = ct
                };

                if (!ulong.TryParse(chatId, out var chatIdParsed))
                {
                    throw new InvalidOperationException("Invalid Discord DM chat ID.");
                }

                var user = await client.GetUserAsync(chatIdParsed, CacheMode.AllowDownload, requestOptions)
                    ?? throw new InvalidOperationException("User not found.");

                EmbedBuilder builder = new EmbedBuilder();

                builder.WithTitle(message.Text);
                builder.WithDescription(message.Body);

                if (!string.IsNullOrWhiteSpace(message.ImageSmall))
                {
                    builder.WithThumbnailUrl(message.ImageSmall);
                }

                if (!string.IsNullOrWhiteSpace(message.ImageLarge))
                {
                    builder.WithImageUrl(message.ImageLarge);
                }

                if (!string.IsNullOrWhiteSpace(message.LinkUrl))
                {
                    builder.WithFields(new EmbedFieldBuilder().WithName(message.LinkText ?? message.LinkUrl).WithValue(message.LinkUrl));
                }

                builder.WithFooter("Sent with Notifo");

                // Throws HttpException if the user has some privacy settings that make it impossible to text them.
                await user.SendMessageAsync(string.Empty, false, builder.Build(), requestOptions);
                break;
            }
            catch (HttpException ex) when (ex.DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
            {
                return DeliveryResult.Failed("User has privacy settings that prevent sending them DMs on Discord.");
            }
            catch
            {
                if (i == Attempts)
                {
                    return DeliveryResult.Failed("Unknown error when sending Discord DM to user.");
                }
            }
        }

        return DeliveryResult.Handled;
    }

    private static string? GetUserId(UserInfo user)
    {
        return UserId.GetString(user.Properties);
    }
}

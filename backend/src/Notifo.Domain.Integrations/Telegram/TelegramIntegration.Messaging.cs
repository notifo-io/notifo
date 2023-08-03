// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Notifo.Domain.Integrations.Resources;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using TelegramChat = Telegram.Bot.Types.Chat;
using TelegramUpdate = Telegram.Bot.Types.Update;
using TelegramUser = Telegram.Bot.Types.User;

namespace Notifo.Domain.Integrations.Telegram;

public sealed partial class TelegramIntegration : IMessagingSender, IIntegrationHook
{
    private const int Attempts = 5;
    private const string TelegramChatId = nameof(TelegramChatId);

    public void AddTargets(IDictionary<string, string> targets, UserInfo user)
    {
        var chatId = GetChatId(user);

        if (!string.IsNullOrWhiteSpace(chatId))
        {
            targets[TelegramChatId] = chatId;
        }
    }

    public async Task<DeliveryResult> SendAsync(IntegrationContext context, MessagingMessage message,
        CancellationToken ct)
    {
        if (!message.Targets.TryGetValue(TelegramChatId, out var chatId))
        {
            return DeliveryResult.Skipped();
        }

        await SendMessageAsync(context, message.Text, chatId, ct);

        return DeliveryResult.Handled;
    }

    private async Task SendMessageAsync(IntegrationContext context, string text, string chatId,
        CancellationToken ct)
    {
        var accessToken = AccessToken.GetString(context.Properties);

        // Try a few attempts to get a non-disposed server instance.
        for (var i = 1; i <= Attempts; i++)
        {
            try
            {
                var client = clientPool.GetBotClient(accessToken);

                await client.SendTextMessageAsync(chatId, text, null, ParseMode.Markdown, cancellationToken: ct);
                break;
            }
            catch (ObjectDisposedException)
            {
                if (i == Attempts)
                {
                    throw;
                }
            }
        }
    }

    public async Task HandleRequestAsync(IntegrationContext context, HttpContext httpContext,
        CancellationToken ct)
    {
        var update = await ParseUpdateAsync(httpContext.Request.Body);

        switch (update.Type)
        {
            case UpdateType.Message when IsUpdate(update):
                await UpdateUser(
                    context,
                    update.Message?.From!,
                    update.Message?.Chat!,
                    default);
                break;
            case UpdateType.MyChatMember:
                var chatId = GetChatId(update.MyChatMember!.Chat);

                await SendMessageAsync(context, GetWelcomeMessage(context), chatId, default);

                await UpdateUser(
                    context,
                    update.MyChatMember.From,
                    update.MyChatMember.Chat,
                    default);
                break;
        }
    }

    private async Task UpdateUser(IntegrationContext context, TelegramUser from, TelegramChat chat,
        CancellationToken ct)
    {
        var chatId = GetChatId(chat);

        var username = from.Username;

        if (string.IsNullOrWhiteSpace(username))
        {
            await SendMessageAsync(context, GetUserNotFoundMessage(context), chatId, ct);
            return;
        }

        var user = await context.IntegrationAdapter.FindUserByPropertyAsync(context.AppId, UserUsername.Name, username, default);
        if (user == null)
        {
            await SendMessageAsync(context, GetUserNotFoundMessage(context), chatId, ct);
            return;
        }

        await context.IntegrationAdapter.UpdateUserAsync(context.AppId, user.Id, TelegramChatId, chatId, default);
    }

    private static string GetChatId(TelegramChat chat)
    {
        return chat.Id.ToString(CultureInfo.InvariantCulture)!;
    }

    private static string GetWelcomeMessage(IntegrationContext context)
    {
        return string.Format(CultureInfo.InvariantCulture, Texts.Telegram_WelcomeMessage, context.AppName);
    }

    private static string GetUserNotFoundMessage(IntegrationContext context)
    {
        return string.Format(CultureInfo.InvariantCulture, Texts.Telegram_UserNotFound, context.AppName);
    }

    private static string? GetChatId(UserInfo user)
    {
        return UserChatId.GetString(user.Properties);
    }

    private static bool IsUpdate(TelegramUpdate update)
    {
        return update.Message is { Type: MessageType.Text, Text: "/update" };
    }

    private static async Task<TelegramUpdate> ParseUpdateAsync(Stream stream)
    {
        using (var streamReader = new StreamReader(stream))
        {
            var json = await streamReader.ReadToEndAsync();

            return JsonConvert.DeserializeObject<TelegramUpdate>(json)!;
        }
    }
}

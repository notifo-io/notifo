// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Notifo.Domain.Channels.Messaging;
using Notifo.Domain.Integrations.Resources;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using TelegramChat = Telegram.Bot.Types.Chat;
using TelegramUpdate = Telegram.Bot.Types.Update;
using TelegramUser = Telegram.Bot.Types.User;

namespace Notifo.Domain.Integrations.Telegram;

public sealed class TelegramMessagingSender : IMessagingSender, IIntegrationHook
{
    private const int Attempts = 5;
    private const string TelegramChatId = nameof(TelegramChatId);
    private readonly IIntegrationAdapter adapter;
    private readonly IMessagingCallback calback;
    private readonly Func<ITelegramBotClient> client;

    public string Name => "Telegram";

    public TelegramMessagingSender(IIntegrationAdapter adapter, IMessagingCallback calback, Func<ITelegramBotClient> client)
    {
        this.adapter = adapter;
        this.calback = calback;
        this.client = client;
    }

    public void AddTargets(MessagingTargets targets, UserContext user)
    {
        var chatId = GetChatId(user);

        if (!string.IsNullOrWhiteSpace(chatId))
        {
            targets[TelegramChatId] = chatId;
        }
    }

    public async Task<MessagingResult> SendAsync(MessagingMessage message,
        CancellationToken ct)
    {
        if (!message.Targets.TryGetValue(TelegramChatId, out var chatId))
        {
            return MessagingResult.Skipped;
        }

        await SendMessageAsync(message.Text, chatId, ct);

        return MessagingResult.Delivered;
    }

    private async Task SendMessageAsync(string text, string chatId,
        CancellationToken ct)
    {
        // Try a few attempts to get a non-disposed server instance.
        for (var i = 1; i <= Attempts; i++)
        {
            try
            {
                await client().SendTextMessageAsync(chatId, text, ParseMode.Markdown, cancellationToken: ct);
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

    public async Task HandleRequestAsync(AppContext app, HttpContext httpContext)
    {
        var update = await ParseUpdateAsync(httpContext.Request.Body);

        switch (update.Type)
        {
            case UpdateType.Message when IsUpdate(update):
                await UpdateUser(
                    app,
                    update.Message?.From!,
                    update.Message?.Chat!,
                    default);
                break;
            case UpdateType.MyChatMember:
                var chatId = GetChatId(update.MyChatMember!.Chat);

                await SendMessageAsync(GetWelcomeMessage(app), chatId, default);

                await UpdateUser(
                    app,
                    update.MyChatMember.From,
                    update.MyChatMember.Chat,
                    default);
                break;
        }
    }

    private async Task UpdateUser(AppContext app, TelegramUser from, TelegramChat chat,
        CancellationToken ct)
    {
        var chatId = GetChatId(chat);

        var username = from.Username;

        if (string.IsNullOrWhiteSpace(username))
        {
            await SendMessageAsync(GetUserNotFoundMessage(app), chatId, ct);
            return;
        }

        var user = await adapter.FindUserByPropertyAsync(app.Id, TelegramIntegration.UserUsername.Name, username, default);

        if (user == null)
        {
            await SendMessageAsync(GetUserNotFoundMessage(app), chatId, ct);
            return;
        }

        await adapter.UpdateUserAsync(app.Id, user.Id, TelegramChatId, chatId, default);
    }

    private static string GetChatId(TelegramChat chat)
    {
        return chat.Id.ToString(CultureInfo.InvariantCulture)!;
    }

    private static string GetWelcomeMessage(AppContext app)
    {
        return string.Format(CultureInfo.InvariantCulture, Texts.Telegram_WelcomeMessage, app.Name);
    }

    private static string GetUserNotFoundMessage(AppContext app)
    {
        return string.Format(CultureInfo.InvariantCulture, Texts.Telegram_UserNotFound, app.Name);
    }

    private static string? GetChatId(UserContext user)
    {
        return TelegramIntegration.UserChatId.GetString(user.Properties);
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

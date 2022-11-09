// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Notifo.Domain.Apps;
using Notifo.Domain.Channels.Messaging;
using Notifo.Domain.Integrations.Resources;
using Notifo.Domain.Users;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using TelegramChat = Telegram.Bot.Types.Chat;
using TelegramUpdate = Telegram.Bot.Types.Update;
using TelegramUser = Telegram.Bot.Types.User;

namespace Notifo.Domain.Integrations.Telegram;

public sealed class TelegramMessagingSender : IMessagingSender
{
    private const int Attempts = 5;
    private const string TelegramChatId = nameof(TelegramChatId);
    private readonly Func<ITelegramBotClient> client;
    private readonly IUserStore userStore;

    public string Name => "Telegram";

    public TelegramMessagingSender(Func<ITelegramBotClient> client, IUserStore userStore)
    {
        this.client = client;
        this.userStore = userStore;
    }

    public bool HasTarget(User user)
    {
        return !string.IsNullOrWhiteSpace(GetChatId(user));
    }

    public Task AddTargetsAsync(MessagingJob job, User user)
    {
        var chatId = GetChatId(user);

        if (!string.IsNullOrWhiteSpace(chatId))
        {
            job.Targets[TelegramChatId] = chatId;
        }

        return Task.CompletedTask;
    }

    public async Task HandleCallbackAsync(App app, HttpContext httpContext)
    {
        var update = await ParseUpdateAsync(httpContext.Request.Body);

        var ct = httpContext.RequestAborted;

        switch (update.Type)
        {
            case UpdateType.Message when IsUpdate(update):
                await UpdateUser(
                    app,
                    update.Message?.From!,
                    update.Message?.Chat!,
                    ct);
                break;
            case UpdateType.MyChatMember:
                {
                    var chatId = GetChatId(update.MyChatMember!.Chat);

                    await SendMessageAsync(GetWelcomeMessage(app), chatId, ct);

                    await UpdateUser(
                        app,
                        update.MyChatMember.From,
                        update.MyChatMember.Chat,
                        ct);
                    break;
                }
        }
    }

    private async Task UpdateUser(App app, TelegramUser from, TelegramChat chat,
        CancellationToken ct)
    {
        var chatId = GetChatId(chat);

        var username = from.Username;

        if (string.IsNullOrWhiteSpace(username))
        {
            await SendMessageAsync(GetUserNotFoundMessage(app), chatId, ct);
            return;
        }

        var user = await userStore.GetByPropertyAsync(app.Id, TelegramIntegration.UserUsername.Name, username, ct);

        if (user == null)
        {
            await SendMessageAsync(GetUserNotFoundMessage(app), chatId, ct);
            return;
        }

        await userStore.UpsertAsync(app.Id, user.Id, new SetUserSystemProperty
        {
            PropertyKey = TelegramIntegration.UserUsername.Name,
            PropertyValue = chatId
        }, ct);

        await SendMessageAsync(GetUserLinkedMessage(app), chatId, ct);
    }

    public async Task<MessagingResult> SendAsync(MessagingJob job, string text,
        CancellationToken ct)
    {
        if (!job.Targets.TryGetValue(TelegramChatId, out var chatId))
        {
            return MessagingResult.Skipped;
        }

        await SendMessageAsync(text, chatId, ct);

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

    private static string GetChatId(TelegramChat chat)
    {
        return chat.Id.ToString(CultureInfo.InvariantCulture)!;
    }

    private static string GetWelcomeMessage(App app)
    {
        return string.Format(CultureInfo.InvariantCulture, Texts.Telegram_WelcomeMessage, app.Name);
    }

    private static string GetUserLinkedMessage(App app)
    {
        return string.Format(CultureInfo.InvariantCulture, Texts.Telegram_UserLinked, app.Name);
    }

    private static string GetUserNotFoundMessage(App app)
    {
        return string.Format(CultureInfo.InvariantCulture, Texts.Telegram_UserNotFound, app.Name);
    }

    private static string? GetChatId(User user)
    {
        return TelegramIntegration.UserChatId.GetString(user.SystemProperties);
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

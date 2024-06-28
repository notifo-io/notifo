// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Notifo.Domain.Integrations.Discord;

[Trait("Category", "Dependencies")]
public sealed class DiscordTests
{
    private readonly ResolvedIntegration<DiscordIntegration> sut;

    public DiscordTests()
    {
        sut = CreateClient();
    }

    [Fact]
    public void Should_get_integration()
    {
        Assert.NotNull(sut.System);
    }

    [Fact]
    public async Task Should_send_simple_message()
    {
        var userId = GetUserId();
        var message = new MessagingMessage
        {
            Text = "Test message",
            Targets = new Dictionary<string, string>() { { DiscordIntegration.DiscordChatId, userId } }
        };

        var result = await sut.System.SendAsync(sut.Context, message, default);

        Assert.Equal(DeliveryResult.Handled, result);
    }

    [Fact]
    public async Task Should_send_full_message()
    {
        var userId = GetUserId();

        var message = new MessagingMessage
        {
            Text = "Test message",
            Body = "Detailed body text",
            Targets = new Dictionary<string, string>() { { DiscordIntegration.DiscordChatId, userId } }
        };

        var result = await sut.System.SendAsync(sut.Context, message, default);

        Assert.Equal(DeliveryResult.Handled, result);
    }

    [Fact]
    public async Task Should_fail_on_user()
    {
        // Random 18-digit number
        var random = new Random();
        string userId = string.Join(string.Empty, Enumerable.Range(0, 18).Select(number => random.Next(0, 9)));

        var message = new MessagingMessage
        {
            Text = "Test message",
            Targets = new Dictionary<string, string>() { { DiscordIntegration.DiscordChatId, userId } }
        };

        var result = await sut.System.SendAsync(sut.Context, message, default);

        Assert.Equal(DeliveryStatus.Failed, result.Status);
    }

    [Fact]
    public async Task Should_accept_images()
    {
        var userId = GetUserId();
        var message = new MessagingMessage
        {
            Text = "Test message",
            ImageSmall = "https://picsum.photos/200/300",
            ImageLarge = "https://picsum.photos/400/600",
            Targets = new Dictionary<string, string>() { { DiscordIntegration.DiscordChatId, userId } }
        };

        var result = await sut.System.SendAsync(sut.Context, message, default);

        Assert.Equal(DeliveryResult.Handled, result);
    }

    [Fact]
    public async Task Should_accept_urls()
    {
        var userId = GetUserId();

        var message = new MessagingMessage
        {
            Text = "Test message",
            LinkUrl = "https://notifo.io",
            LinkText = "Notifo",
            Targets = new Dictionary<string, string>() { { DiscordIntegration.DiscordChatId, userId } }
        };
        var result = await sut.System.SendAsync(sut.Context, message, default);

        Assert.Equal(DeliveryResult.Handled, result);
    }

    private static ResolvedIntegration<DiscordIntegration> CreateClient()
    {
        var botToken = TestHelpers.Configuration.GetValue<string>("discord:botToken")!;

        var context = BuildContext(new Dictionary<string, string>
        {
            [DiscordIntegration.BotToken.Name] = botToken,
        });

        var integration =
            new ServiceCollection()
                .AddIntegrationDiscord()
                .AddMemoryCache()
                .BuildServiceProvider()
                .GetRequiredService<DiscordIntegration>();

        return new ResolvedIntegration<DiscordIntegration>(Guid.NewGuid().ToString(), context, integration);
    }

    private string GetUserId()
    {
        var id = TestHelpers.Configuration.GetValue<string>("discord:userId");

        ArgumentException.ThrowIfNullOrEmpty(id, nameof(id));
        return id;
    }

    private static IntegrationContext BuildContext(Dictionary<string, string> properties)
    {
        return new IntegrationContext
        {
            UpdateStatusAsync = null!,
            AppId = string.Empty,
            AppName = string.Empty,
            CallbackToken = string.Empty,
            CallbackUrl = string.Empty,
            IntegrationAdapter = null!,
            IntegrationId = string.Empty,
            Properties = properties,
            WebhookUrl = string.Empty,
        };
    }
}

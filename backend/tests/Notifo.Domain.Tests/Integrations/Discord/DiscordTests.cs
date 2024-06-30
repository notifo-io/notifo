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
    private readonly string userId;
    private readonly ResolvedIntegration<DiscordIntegration> sut;

    public DiscordTests()
    {
        userId = TestHelpers.Configuration.GetValue<string>("discord:userId") ??
            throw new InvalidOperationException("Configured user Id with 'discord:userId'");

        var botToken = TestHelpers.Configuration.GetValue<string>("discord:botToken") ??
            throw new InvalidOperationException("Configured bot token with 'discord:botToken'");

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

        sut = new ResolvedIntegration<DiscordIntegration>(Guid.NewGuid().ToString(), context, integration);
    }

    [Fact]
    public void Should_get_integration()
    {
        Assert.NotNull(sut.System);
    }

    [Fact]
    public async Task Should_send_simple_message()
    {
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
        var message = new MessagingMessage
        {
            Text = "Test message",
            Body = "Detailed body text",
            Targets = new Dictionary<string, string>()
            {
                { DiscordIntegration.DiscordChatId, userId }
            }
        };

        var result = await sut.System.SendAsync(sut.Context, message, default);

        Assert.Equal(DeliveryResult.Handled, result);
    }

    [Fact]
    public async Task Should_fail_on_user()
    {
        var invalidUser = Guid.NewGuid().ToString();

        var message = new MessagingMessage
        {
            Text = "Test message",
            Targets = new Dictionary<string, string>()
            {
                { DiscordIntegration.DiscordChatId, invalidUser }
            }
        };

        var result = await sut.System.SendAsync(sut.Context, message, default);

        Assert.Equal(DeliveryStatus.Failed, result.Status);
    }

    [Fact]
    public async Task Should_accept_images()
    {
        var message = new MessagingMessage
        {
            Text = "Test message",
            ImageSmall = "https://picsum.photos/200/300",
            ImageLarge = "https://picsum.photos/400/600",
            Targets = new Dictionary<string, string>()
            {
                { DiscordIntegration.DiscordChatId, userId }
            }
        };

        var result = await sut.System.SendAsync(sut.Context, message, default);

        Assert.Equal(DeliveryResult.Handled, result);
    }

    [Fact]
    public async Task Should_accept_urls()
    {
        var message = new MessagingMessage
        {
            Text = "Test message",
            LinkUrl = "https://notifo.io",
            LinkText = "Notifo",
            Targets = new Dictionary<string, string>()
            {
                { DiscordIntegration.DiscordChatId, userId }
            }
        };

        var result = await sut.System.SendAsync(sut.Context, message, default);

        Assert.Equal(DeliveryResult.Handled, result);
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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Notifo.Domain.Integrations.Discord;
public sealed class DiscordTests
{
    [Fact]
    public void Should_get_integration()
    {
        var client = CreateClient();
        Assert.NotNull(client.System);
    }

    [Fact]
    public async Task Should_send_simple_message()
    {
        var client = CreateClient();

        var message = new MessagingMessage
        {
            Text = "Test message",
        };

        var userId = GetUserId();
        AddTargets(message, userId);

        var result = await client.System.SendAsync(client.Context, message, default);

        Assert.Equal(DeliveryResult.Handled, result);
    }

    [Fact]
    public async Task Send_full_message()
    {
        var client = CreateClient();

        var message = new MessagingMessage
        {
            Text = "Test message",
            DetailedBodyText = "Detailed body text",
        };

        var userId = GetUserId();
        AddTargets(message, userId);
        var result = await client.System.SendAsync(client.Context, message, default);

        Assert.Equal(DeliveryResult.Handled, result);
    }

    [Fact]
    public async Task Should_failed_on_user_without_app_installed()
    {
        var client = CreateClient();

        var message = new MessagingMessage { Text = "Test message" };

        var userId = "261155655006000000"; // Random userId
        AddTargets(message, userId);

        var result = await client.System.SendAsync(client.Context, message, default);

        Assert.Equal(DeliveryStatus.Failed, result.Status);
    }

    [Fact]
    public async Task Should_accept_images()
    {
        var client = CreateClient();

        var message = new MessagingMessage
        {
            Text = "Test message",
            ImageSmall = "https://picsum.photos/200/300",
            ImageLarge = "https://picsum.photos/400/600"
        };

        var userId = GetUserId();
        AddTargets(message, userId);
        var result = await client.System.SendAsync(client.Context, message, default);

        Assert.Equal(DeliveryResult.Handled, result);
    }

    [Fact]
    public async Task Should_accept_urls()
    {
        var client = CreateClient();

        var message = new MessagingMessage
        {
            Text = "Test message",
            LinkUrl = "https://notifo.io",
            LinkText = "Notifo"
        };
        var userId = GetUserId();
        AddTargets(message, userId);
        var result = await client.System.SendAsync(client.Context, message, default);

        Assert.Equal(DeliveryResult.Handled, result);
    }

    static ResolvedIntegration<DiscordIntegration> CreateClient()
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

        Assert.NotNull(integration);
        return new ResolvedIntegration<DiscordIntegration>(Guid.NewGuid().ToString(), context, integration);
    }

    private string GetUserId()
    {
        var id = TestHelpers.Configuration.GetValue<string>("discord:userId");

        Assert.False(string.IsNullOrWhiteSpace(id), "Please set the Discord userId in the testing env");
        return id;
    }

    private MessagingMessage AddTargets(MessagingMessage message, string userId)
    {
        message.Targets = new Dictionary<string, string>()
        {
            { DiscordIntegration.DiscordChatId, userId }
        };
        return message;
    }

    static IntegrationContext BuildContext(Dictionary<string, string> properties)
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

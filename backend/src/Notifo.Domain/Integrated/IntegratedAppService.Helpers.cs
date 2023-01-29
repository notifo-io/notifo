// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Apps;
using Notifo.Domain.Identity;
using Notifo.Domain.Integrations;
using Notifo.Domain.Subscriptions;
using Notifo.Domain.Users;
using Squidex.Hosting;

namespace Notifo.Domain.Integrated;

public sealed partial class IntegratedAppService : IInitializable
{
    private static readonly string IntegratedAppId = Guid.Empty.ToString();

    public int Order => int.MaxValue;

    public async Task InitializeAsync(
        CancellationToken ct)
    {
        var integratedApp = await appStore.GetAsync(IntegratedAppId, ct);

        if (integratedApp != null)
        {
            return;
        }

        await CreateAppAsync(ct);

        await foreach (var user in userResolver.QueryAllAsync(ct))
        {
            if (user.Roles.Contains(NotifoRoles.HostAdmin))
            {
                await AddContributorAsync(user.Id, ct);
            }
        }

        var resolvedUsers = new Dictionary<string, IUser?>();

        await foreach (var app in appStore.QueryAllAsync(ct))
        {
            foreach (var contributor in app.Contributors)
            {
                var userId = contributor.Key;

                if (!resolvedUsers.TryGetValue(userId, out var user))
                {
                    user = await userResolver.FindByIdAsync(userId, ct);

                    if (user != null)
                    {
                        await CreateUserAsync(user, ct);
                    }

                    resolvedUsers[userId] = user;
                }

                if (user != null)
                {
                    await SubscribeAsync(app.Id, userId, ct);
                }
            }
        }
    }

    private async Task CreateAppAsync(CancellationToken ct)
    {
        var command = new UpsertApp
        {
            Name = "Notifo"
        };

        await SendAsync(command, null, ct);
    }

    private async Task AddContributorAsync(string userId,
        CancellationToken ct)
    {
        var command = new AddContributor
        {
            EmailOrId = userId,
        };

        await SendAsync(command, null, ct);
    }

    private async Task RemoveContributorAsync(string userId,
        CancellationToken ct)
    {
        var command = new RemoveContributor
        {
            ContributorId = userId
        };

        await SendAsync(command, userId, ct);
    }

    private async Task SubscribeAsync(string appId, string userId,
        CancellationToken ct)
    {
        var topic = CreateTopic(appId);

        var updateCommand = new AddUserAllowedTopic
        {
            Prefix = topic,
        };

        await SendAsync(updateCommand, userId, ct);

        var subscribeCommand = new Subscribe
        {
            Topic = CreateTopic(appId)
        };

        await SendAsync(subscribeCommand, userId, ct);
    }

    private async Task UnsubscribeAsync(string appId, string userId,
        CancellationToken ct)
    {
        var topic = CreateTopic(appId);

        var updateCommand = new RemoveUserAllowedTopic
        {
            Prefix = topic,
        };

        await SendAsync(updateCommand, userId, ct);

        var unsubscribeCommand = new Subscribe
        {
            Topic = CreateTopic(appId)
        };

        await SendAsync(unsubscribeCommand, userId, ct);
    }

    private Task CreateUserAsync(IUser user,
        CancellationToken ct)
    {
        var userCommand = new UpsertUser
        {
            UserId = user.Id,
            EmailAddress = user.Email,
            PreferredLanguage = "en",
            PreferredTimezone = "UTC",
            Settings = new ChannelSettings
            {
                [Providers.WebPush] = new ChannelSetting
                {
                    Send = ChannelSend.Send
                },
                [Providers.Email] = new ChannelSetting
                {
                    Send = ChannelSend.Send
                },
            },
            RequiresWhitelistedTopics = true
        };

        return SendAsync(userCommand, user.Id, ct);
    }

    private Task SendAsync(CommandBase request, string? userId,
        CancellationToken ct = default)
    {
        request.With(IntegratedAppId, userId ?? IntegratedAppId);

        return mediator.PublishAsync(request, ct);
    }

    private static string CreateTopic(string appId)
    {
        return $"apps/{appId}";
    }

    private static bool IsAdmin(IUser user)
    {
        return user.Roles.Contains(NotifoRoles.HostAdmin);
    }
}

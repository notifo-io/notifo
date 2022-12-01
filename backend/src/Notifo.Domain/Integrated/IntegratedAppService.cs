// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using NodaTime;
using Notifo.Domain.Apps;
using Notifo.Domain.Events;
using Notifo.Domain.Identity;
using Notifo.Domain.Log;
using Notifo.Domain.Resources;
using Notifo.Domain.Templates;
using Notifo.Domain.Users;
using Notifo.Infrastructure.Mediator;
using Notifo.Infrastructure.Texts;
using Squidex.Hosting;

namespace Notifo.Domain.Integrated;

public sealed partial class IntegratedAppService :
    IIntegratedAppService,
    IMessageMiddleware<AddContributor>,
    IMessageMiddleware<DeleteTemplate>,
    IMessageMiddleware<FirstLogCreated>,
    IMessageMiddleware<RemoveContributor>,
    IMessageMiddleware<UserDeleted>,
    IMessageMiddleware<UserRegistered>,
    IMessageMiddleware<UserUpdated>
{
    private readonly IAppStore appStore;
    private readonly IMediator mediator;
    private readonly IUrlGenerator urlGenerator;
    private readonly IEventPublisher eventPublisher;
    private readonly IUserStore userStore;
    private readonly IUserResolver userResolver;
    private readonly ILogger<IntegratedAppService> log;

    public IntegratedAppService(
        IAppStore appStore,
        IEventPublisher eventPublisher,
        IMediator mediator,
        IUrlGenerator urlGenerator,
        IUserStore userStore,
        IUserResolver userResolver,
        ILogger<IntegratedAppService> log)
    {
        this.appStore = appStore;
        this.mediator = mediator;
        this.urlGenerator = urlGenerator;
        this.eventPublisher = eventPublisher;
        this.userStore = userStore;
        this.userResolver = userResolver;
        this.log = log;
    }

    public async Task<string?> GetTokenAsync(string userId,
        CancellationToken ct = default)
    {
        var user = await userStore.GetAsync(IntegratedAppId, userId, ct);

        return user?.ApiKey;
    }

    public ValueTask<object?> HandleAsync(UserRegistered request, NextDelegate next,
        CancellationToken ct)
    {
        return HandleMessageAsync(request, next, async (request, ct) =>
        {
            await CreateUserAsync(request.User, ct);

            if (IsAdmin(request.User))
            {
                await AddContributorAsync(request.User.Id, ct);
            }
        }, ct);
    }

    public ValueTask<object?> HandleAsync(UserUpdated request, NextDelegate next,
        CancellationToken ct)
    {
        return HandleMessageAsync(request, next, async (request, ct) =>
        {
            if (IsAdmin(request.User) && !IsAdmin(request.OldUser))
            {
                await AddContributorAsync(request.User.Id, ct);
            }

            if (!IsAdmin(request.User) && IsAdmin(request.OldUser))
            {
                await RemoveContributorAsync(request.User.Id, ct);
            }
        }, ct);
    }

    public ValueTask<object?> HandleAsync(UserDeleted request, NextDelegate next,
        CancellationToken ct)
    {
        return HandleMessageAsync(request, next, async (request, ct) =>
        {
            if (IsAdmin(request.User))
            {
                await RemoveContributorAsync(request.User.Id, ct);
            }
        }, ct);
    }

    public ValueTask<object?> HandleAsync(AddContributor request, NextDelegate next,
        CancellationToken ct)
    {
        return HandleMessageAsync(request, next, async (request, ct) =>
        {
            await SubscribeAsync(request.AppId, request.EmailOrId, ct);

            var user = await userResolver.FindByIdAsync(request.EmailOrId, ct);

            if (user == null)
            {
                return;
            }

            await PublishAsync(
                request.AppId,
                request.PrincipalId,
                "app/{app}/settings",
                Texts.NotificationContributorCreated,
                new NotificationProperties
                {
                    ["user"] = user.Email
                },
                ct);
        }, ct);
    }

    public ValueTask<object?> HandleAsync(RemoveContributor request, NextDelegate next,
        CancellationToken ct)
    {
        return HandleMessageAsync(request, next, async (request, ct) =>
        {
            await UnsubscribeAsync(request.AppId, request.ContributorId, ct);

            var user = await userResolver.FindByIdAsync(request.ContributorId, ct);

            if (user == null)
            {
                return;
            }

            await PublishAsync(
                request.AppId,
                request.PrincipalId,
                "app/{app}/settings",
                Texts.NotificationContributorRemoved,
                new NotificationProperties
                {
                    ["user"] = user.Email
                }, ct);
        }, ct);
    }

    public ValueTask<object?> HandleAsync(FirstLogCreated request, NextDelegate next,
        CancellationToken ct)
    {
        return HandleMessageAsync(request, next, async (request, ct) =>
        {
            await PublishAsync(
                request.Entry.AppId,
                null,
                "app/{app}/log",
                Texts.NotificationFirstLog,
                new NotificationProperties
                {
                    ["log"] = request.Entry.Message
                }, ct);
        }, ct);
    }

    public ValueTask<object?> HandleAsync(DeleteTemplate request, NextDelegate next,
        CancellationToken ct)
    {
        return HandleMessageAsync(request, next, async (request, ct) =>
        {
            await PublishAsync(
                request.AppId,
                request.PrincipalId,
                "/app/{app}/templates",
                Texts.NotificationTemplateDeleted,
                new NotificationProperties
                {
                    ["code"] = request.TemplateCode
                },
                ct);
        }, ct);
    }

    private async Task PublishAsync(string appId, string? creator, string link, string message, NotificationProperties? properties,
        CancellationToken ct)
    {
        var app = await appStore.GetCachedAsync(appId, ct);

        // A user can be part of multiple apps. Therefore we need to resolve the app name.
        if (app == null)
        {
            return;
        }

        properties ??= new NotificationProperties();
        properties["app"] = app.Name;

        var publish = new EventMessage
        {
            AppId = IntegratedAppId,
            CreatorId = creator,
            Created = SystemClock.Instance.GetCurrentInstant(),
            Topic = CreateTopic(appId),
            Formatting = new NotificationFormatting<LocalizedText>
            {
                Subject = new LocalizedText
                {
                    ["en"] = message
                },
                LinkText = new LocalizedText
                {
                    ["en"] = Texts.NotificationLink,
                },
                LinkUrl = new LocalizedText
                {
                    ["en"] = BuildLink(appId, link)!
                }
            },
            Properties = properties,
        };

        await eventPublisher.PublishAsync(publish, ct);
    }

    private string? BuildLink(string appId, string? link)
    {
        if (string.IsNullOrWhiteSpace(link))
        {
            return null;
        }

        return urlGenerator.BuildUrl(link.Replace("{app}", appId, StringComparison.Ordinal));
    }

    private async ValueTask<object?> HandleMessageAsync<T>(T request, NextDelegate next, Func<T, CancellationToken, ValueTask> action,
        CancellationToken ct) where T : notnull
    {
        // Call next first for consistency. It is not a command, so it should not matter.
        var result = await next(request, ct);

        if (request is AppCommand appCommand && appCommand.AppId == IntegratedAppId)
        {
            return result;
        }

        // Run in an extra task to not block the actual HTTP call.
        await Task.Run(async () =>
        {
            try
            {
                // Do not pass cancellation token.
                await action(request, default);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Failed to run sync command.");
            }
        }, default);

        return result;
    }
}

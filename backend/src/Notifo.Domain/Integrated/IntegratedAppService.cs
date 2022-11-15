// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using Notifo.Domain.Apps;
using Notifo.Domain.Events;
using Notifo.Domain.Identity;
using Notifo.Domain.Log;
using Notifo.Domain.Resources;
using Notifo.Domain.Templates;
using Notifo.Domain.Users;
using Notifo.Infrastructure.Mediator;

namespace Notifo.Domain.Integrated;

public sealed partial class IntegratedAppService :
    IIntegratedAppService,
    IMessageMiddleware<AddContributor>,
    IMessageMiddleware<DeleteTemplate>,
    IMessageMiddleware<FirstLogCreated>,
    IMessageMiddleware<RemoveContributor>,
    IMessageMiddleware<UpsertTemplate>,
    IMessageMiddleware<UserDeleted>,
    IMessageMiddleware<UserRegistered>,
    IMessageMiddleware<UserUpdated>
{
    private readonly IAppStore appStore;
    private readonly IMediator mediator;
    private readonly IEventPublisher eventPublisher;
    private readonly IUserStore userStore;
    private readonly IUserResolver userResolver;
    private readonly ILogger<IntegratedAppService> log;

    public IntegratedAppService(
        IAppStore appStore,
        IEventPublisher eventPublisher,
        IMediator mediator,
        IUserStore userStore,
        IUserResolver userResolver,
        ILogger<IntegratedAppService> log)
    {
        this.appStore = appStore;
        this.mediator = mediator;
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

            if (user != null)
            {
                var properties = new NotificationProperties
                {
                    ["user"] = user.Email
                };

                await PublishAsync(request.AppId, request.PrincipalId, Texts.NotificationContributorCreated, properties, ct);
            }
        }, ct);
    }

    public ValueTask<object?> HandleAsync(RemoveContributor request, NextDelegate next,
        CancellationToken ct)
    {
        return HandleMessageAsync(request, next, async (request, ct) =>
        {
            await UnsubscribeAsync(request.AppId, request.ContributorId, ct);

            var user = await userResolver.FindByIdAsync(request.ContributorId, ct);

            if (user != null)
            {
                var properties = new NotificationProperties
                {
                    ["user"] = user.Email
                };

                await PublishAsync(request.AppId, request.PrincipalId, Texts.NotificationContributorRemoved, properties, ct);
            }
        }, ct);
    }

    public ValueTask<object?> HandleAsync(FirstLogCreated request, NextDelegate next,
        CancellationToken ct)
    {
        return HandleMessageAsync(request, next, async (request, ct) =>
        {
            var properties = new NotificationProperties
            {
                ["log"] = request.Entry.Message
            };

            await PublishAsync(request.Entry.AppId, null, Texts.NotificationFirstLog, properties, ct);
        }, ct);
    }

    public ValueTask<object?> HandleAsync(UpsertTemplate request, NextDelegate next,
        CancellationToken ct)
    {
        return HandleMessageAsync(request, next, async (request, ct) =>
        {
            var properties = new NotificationProperties
            {
                ["code"] = request.TemplateCode
            };

            await PublishAsync(request.AppId, request.PrincipalId, Texts.NotificationTemplateUpserted, properties, ct);
        }, ct);
    }

    public ValueTask<object?> HandleAsync(DeleteTemplate request, NextDelegate next,
        CancellationToken ct)
    {
        return HandleMessageAsync(request, next, async (request, ct) =>
        {
            var properties = new NotificationProperties
            {
                ["code"] = request.TemplateCode
            };

            await PublishAsync(request.AppId, request.PrincipalId, Texts.NotificationTemplateDeleted, properties, ct);
        }, ct);
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

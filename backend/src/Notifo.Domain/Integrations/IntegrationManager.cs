// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Google.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Notifo.Domain.Apps;
using Notifo.Domain.Channels.Messaging;
using Notifo.Domain.Resources;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Mediator;
using Notifo.Infrastructure.Timers;
using Notifo.Infrastructure.Validation;
using Squidex.Hosting;
using IIntegrationRegistries = System.Collections.Generic.IEnumerable<Notifo.Domain.Integrations.IIntegrationRegistry>;

namespace Notifo.Domain.Integrations;

public sealed class IntegrationManager : IIntegrationManager, IBackgroundProcess
{
    private readonly IAppStore appStore;
    private readonly IIntegrationAdapter integrationAdapter;
    private readonly IIntegrationRegistries integrationRegistries;
    private readonly IIntegrationUrl integrationUrl;
    private readonly ILogger<IntegrationManager> log;
    private readonly IMediator mediator;
    private readonly Lazy<ICallback<ISmsSender>> callbackSms;
    private readonly Lazy<ICallback<IMessagingSender>> callbackMessaging;
    private readonly ConditionEvaluator conditionEvaluator;
    private CompletionTimer? timer;

    public IEnumerable<IntegrationDefinition> Definitions
    {
        get => integrationRegistries.SelectMany(x => x.Integrations).Select(x => x.Definition);
    }

    public IntegrationManager(
        IAppStore appStore,
        IIntegrationAdapter integrationAdapter,
        IIntegrationRegistries integrationRegistries,
        IIntegrationUrl integrationUrl,
        IServiceProvider serviceProvider,
        IMediator mediator,
        ILogger<IntegrationManager> log)
    {
        this.appStore = appStore;
        this.integrationAdapter = integrationAdapter;
        this.mediator = mediator;
        this.integrationRegistries = integrationRegistries;
        this.integrationUrl = integrationUrl;
        this.log = log;

        callbackSms = new Lazy<ICallback<ISmsSender>>(() =>
        {
            return serviceProvider.GetRequiredService<ICallback<ISmsSender>>();
        });

        callbackMessaging = new Lazy<ICallback<IMessagingSender>>(() =>
        {
            return serviceProvider.GetRequiredService<ICallback<IMessagingSender>>();
        });

        conditionEvaluator = new ConditionEvaluator(log);
    }

    public Task StartAsync(
        CancellationToken ct)
    {
        timer = new CompletionTimer(5000, CheckAsync, 5000);

        return Task.CompletedTask;
    }

    public Task StopAsync(
        CancellationToken ct)
    {
        return timer?.StopAsync() ?? Task.CompletedTask;
    }

    public Task<IntegrationStatus> OnInstallAsync(string id, App app, ConfiguredIntegration configured, ConfiguredIntegration? previous,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(id);
        Guard.NotNull(app);
        Guard.NotNull(configured);

        var integration = GetIntegrationOrThrow(configured.Type);

        if (configured.Properties.EqualsDictionary(previous?.Properties))
        {
            return Task.FromResult(IntegrationStatus.Verified);
        }

        List<ValidationError>? errors = null;

        foreach (var property in integration.Definition.Properties)
        {
            var input = configured.Properties.GetValueOrDefault(property.Name);

            if (!property.IsValid(input, out var error))
            {
                errors ??= new List<ValidationError>();
                errors.Add(new ValidationError(error, property.Name));
            }
        }

        if (errors != null)
        {
            throw new ValidationException(errors);
        }

        return integration.OnConfiguredAsync(BuildContext(app, id, integration, configured), previous, ct);
    }

    public Task OnCallbackAsync(string id, App app, HttpContext httpContext,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(id);
        Guard.NotNull(app);

        var (_, context, hook) = Resolve<IIntegrationHook>(id, app);

        if (hook == null)
        {
            return Task.CompletedTask;
        }

        return hook.HandleRequestAsync(context, httpContext, ct);
    }

    public Task OnRemoveAsync(string id, App app, ConfiguredIntegration configured,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(id);
        Guard.NotNull(app);

        if (!app.Integrations.TryGetValue(id, out var _))
        {
            return Task.CompletedTask;
        }

        var integration = GetIntegrationOrThrow(configured.Type);

        return integration.OnRemovedAsync(BuildContext(app, id, integration, configured), ct);
    }

    public bool HasIntegration<T>(App app)
    {
        Guard.NotNull(app);

        var configureds = app.Integrations;

        foreach (var (_, configured) in configureds)
        {
            if (!IsReady(configured))
            {
                continue;
            }

            var integration = GetIntegration(configured.Type);

            if (integration is T)
            {
                return true;
            }
        }

        return false;
    }

    public IEnumerable<ResolvedIntegration<T>> Resolve<T>(App app, IIntegrationTarget? target)
    {
        Guard.NotNull(app);

        var configureds = app.Integrations;

        foreach (var (id, configured) in configureds)
        {
            if (!IsReady(configured))
            {
                continue;
            }

            if (target != null && (!IsMatchingTest(configured, target) || !IsMatchingCondition(configured, target)))
            {
                continue;
            }

            var integration = GetIntegration(configured.Type);

            if (integration is not T typed)
            {
                continue;
            }

            yield return new ResolvedIntegration<T>(id, BuildContext(app, id, integration, configured), typed);
        }
    }

    public ResolvedIntegration<T> Resolve<T>(string id, App app) where T : class
    {
        Guard.NotNullOrEmpty(id);
        Guard.NotNull(app);

        if (!app.Integrations.TryGetValue(id, out var configured))
        {
            return default;
        }

        var integration = GetIntegration(configured.Type);

        if (integration is not T typed)
        {
            return default;
        }

        return new ResolvedIntegration<T>(id, BuildContext(app, id, integration, configured), typed);
    }

    public async Task CheckAsync(
        CancellationToken ct)
    {
        try
        {
            var apps = await appStore.QueryWithPendingIntegrationsAsync(ct);

            if (apps.Count == 0)
            {
                return;
            }

            foreach (var app in apps)
            {
                var updates = new Dictionary<string, IntegrationStatus>();

                foreach (var (id, configured) in app.Integrations)
                {
                    var status = configured.Status;

                    if (status != IntegrationStatus.Pending)
                    {
                        continue;
                    }

                    var integration = GetIntegration(configured.Type);

                    if (integration == null)
                    {
                        updates[id] = IntegrationStatus.Verified;
                        continue;
                    }

                    IntegrationStatus newStatus = configured.Status;
                    try
                    {
                        await integration.CheckStatusAsync(BuildContext(app, id, integration, configured), ct);
                    }
                    catch (Exception ex)
                    {
                        log.LogError(ex, "Check integrations failed.");
                    }

                    if (status != configured.Status)
                    {
                        updates[id] = IntegrationStatus.Verified;
                    }
                }

                if (updates.Count > 0)
                {
                    var command = new UpdateAppIntegrationStatus { AppId = app.Id, Status = updates };

                    await mediator.SendAsync(command, ct);
                }
            }
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Check integrations failed.");
        }
    }

    private IIntegration GetIntegrationOrThrow(string type)
    {
        var integration = GetIntegration(type);

        if (integration == null)
        {
            var error = string.Format(CultureInfo.InvariantCulture, Texts.IntegrationNotFound, type);

            throw new ValidationException(error);
        }

        return integration;
    }

    private IIntegration? GetIntegration(string type)
    {
        foreach (var registry in integrationRegistries)
        {
            if (registry.TryGetIntegration(type, out var integration))
            {
                return integration;
            }
        }

        return null;
    }

    private static bool IsReady(ConfiguredIntegration configured)
    {
        return configured.Enabled && configured.Status == IntegrationStatus.Verified;
    }

    private static bool IsMatchingTest(ConfiguredIntegration configured, IIntegrationTarget target)
    {
        return configured.Test == null || configured.Test.Value == target.Test;
    }

    private bool IsMatchingCondition(ConfiguredIntegration configured, IIntegrationTarget target)
    {
        return conditionEvaluator.Evaluate(configured.Condition, target);
    }

    private IntegrationContext BuildContext(App app, string id,  IIntegration integration, ConfiguredIntegration configured)
    {
        var updateStatus = new UpdateStatus((trackingToken, result) =>
        {
            switch (integration)
            {
                case ISmsSender sms:
                    return callbackSms.Value.UpdateStatusAsync(sms, trackingToken, result);
                case IMessagingSender messaging:
                    return callbackMessaging.Value.UpdateStatusAsync(messaging, trackingToken, result);
                default:
                    return Task.CompletedTask;
            }
        });

        return new IntegrationContext
        {
            UpdateStatusAsync = updateStatus,
            AppId = app.Id,
            AppName = app.Name,
            CallbackToken = string.Empty,
            CallbackUrl = integrationUrl.CallbackUrl(),
            IntegrationAdapter = integrationAdapter,
            IntegrationId = id,
            Properties = new Dictionary<string, string>(configured.Properties),
            WebhookUrl = integrationUrl.WebhookUrl(app.Id, id)
        };
    }
}

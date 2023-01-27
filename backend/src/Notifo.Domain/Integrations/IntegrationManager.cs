// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Notifo.Domain.Apps;
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
    private readonly IIntegrationRegistries integrationRegistries;
    private readonly IIntegrationUrl integrationUrl;
    private readonly IAppStore appStore;
    private readonly IMediator mediator;
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<IntegrationManager> log;
    private readonly ConditionEvaluator conditionEvaluator;
    private CompletionTimer timer;

    public IEnumerable<IntegrationDefinition> Definitions
    {
        get => integrationRegistries.SelectMany(x => x.Integrations).Select(x => x.Definition);
    }

    public IntegrationManager(
        IAppStore appStore,
        IIntegrationRegistries integrationRegistries,
        IIntegrationUrl integrationUrl,
        IMediator mediator,
        IServiceProvider serviceProvider,
        ILogger<IntegrationManager> log)
    {
        this.appStore = appStore;
        this.mediator = mediator;
        this.integrationRegistries = integrationRegistries;
        this.integrationUrl = integrationUrl;
        this.serviceProvider = serviceProvider;
        this.log = log;

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
        return timer.StopAsync();
    }

    public Task<IntegrationStatus> OnInstallAsync(string id, App app, ConfiguredIntegration configured, ConfiguredIntegration? previous,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(id);
        Guard.NotNull(app);
        Guard.NotNull(configured);

        var integration = GetIntegrationOrThrow(configured.Type);

        var errors = new List<ValidationError>();

        foreach (var property in integration.Definition.Properties)
        {
            var value = configured.Properties.GetValueOrDefault(property.Name);

            foreach (var error in property.Validate(value))
            {
                errors.Add(new ValidationError(error, property.Name));
            }
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }

        return integration.OnConfiguredAsync(BuildContext(app, id, configured), previous, ct);
    }

    public Task OnCallbackAsync(string id, App app, HttpContext httpContext,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(id);
        Guard.NotNull(app);

        if (!app.Integrations.TryGetValue(id, out var configured))
        {
            return Task.CompletedTask;
        }

        var integration = GetIntegrationOrThrow(configured.Type);

        return integration.HandleWebhookAsync(BuildContext(app, id, configured), httpContext, ct);
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

        return integration.OnRemovedAsync(BuildContext(app, id, configured), ct);
    }

    public T? Resolve<T>(string id, App app) where T : class
    {
        Guard.NotNullOrEmpty(id);
        Guard.NotNull(app);

        if (!app.Integrations.TryGetValue(id, out var configured))
        {
            return null;
        }

        var integration = GetIntegration(configured.Type);

        if (integration == null)
        {
            return null;
        }

        return integration.Create(typeof(T), BuildContext(app, id, configured), serviceProvider) as T;
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
                        await integration.CheckStatusAsync(BuildContext(app, id, configured), ct);
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

    private IntegrationContext BuildContext(App app, string id, ConfiguredIntegration configured)
    {
        return new IntegrationContext
        {
            AppId = app.Id,
            AppName = app.Name,
            CallbackToken = string.Empty,
            CallbackUrl = integrationUrl.CallbackUrl(),
            IntegrationId = id,
            Properties = configured.Properties,
            WebhookUrl = integrationUrl.WebhookUrl(app.Id, id)
        };
    }

    public IEnumerable<(T Integration, string Id)> GetMatchingIntegrations<T>(App app, IIntegrationTarget target)
    {
        var configureds = app.Integrations;

        foreach (var (id, configured) in configureds)
        {
            if (!IsReady(configured))
            {
                continue;
            }

            if (!IsMatchingTest(configured, target) || !IsMatchingCondition(configured, target))
            {
                continue;
            }

            var integration = GetIntegration(configured.Type);

            if (integration == null)
            {
                continue;
            }

            yield break;
        }
    }

    public IEnumerable<string> GetMatchingIntegrationIds<T>(App app, IIntegrationTarget target)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<(string Id, T Integration)> Resolve<T>(App app, IIntegrationTarget? target)
    {
        throw new NotImplementedException();
    }

    public bool HasIntegration<T>(App app)
    {
        throw new NotImplementedException();
    }
}

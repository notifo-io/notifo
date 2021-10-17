// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Notifo.Domain.Apps;
using Notifo.Domain.Resources;
using Notifo.Infrastructure.Timers;
using Notifo.Infrastructure.Validation;
using Squidex.Hosting;
using Squidex.Log;

namespace Notifo.Domain.Integrations
{
    public sealed class IntegrationManager : IIntegrationManager, IInitializable
    {
        private readonly IEnumerable<IIntegration> appIntegrations;
        private readonly IAppStore appStore;
        private readonly IServiceProvider serviceProvider;
        private readonly ISemanticLog log;
        private CompletionTimer timer;

        public IEnumerable<IntegrationDefinition> Definitions
        {
            get => appIntegrations.Select(x => x.Definition);
        }

        public IntegrationManager(IEnumerable<IIntegration> appIntegrations, IAppStore appStore, IServiceProvider serviceProvider,
            ISemanticLog log)
        {
            this.appIntegrations = appIntegrations;
            this.appStore = appStore;
            this.serviceProvider = serviceProvider;
            this.log = log;
        }

        public Task InitializeAsync(
            CancellationToken ct)
        {
            timer = new CompletionTimer(5000, CheckAsync, 5000);

            return Task.CompletedTask;
        }

        public Task ReleaseAsync(
            CancellationToken ct)
        {
            return timer.StopAsync();
        }

        public Task ValidateAsync(ConfiguredIntegration configured,
            CancellationToken ct = default)
        {
            var integration = appIntegrations.FirstOrDefault(x => x.Definition.Type == configured.Type);

            if (integration == null)
            {
                var error = string.Format(CultureInfo.InvariantCulture, Texts.IntegrationNotFound, configured.Type);

                throw new ValidationException(error);
            }

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

            return Task.CompletedTask;
        }

        public Task HandleConfiguredAsync(string id, App app, ConfiguredIntegration configured, ConfiguredIntegration? previous,
            CancellationToken ct = default)
        {
            var integration = appIntegrations.FirstOrDefault(x => x.Definition.Type == configured.Type);

            if (integration == null)
            {
                var error = string.Format(CultureInfo.InvariantCulture, Texts.IntegrationNotFound, configured.Type);

                throw new ValidationException(error);
            }

            return integration.OnConfiguredAsync(app, id, configured, previous, ct);
        }

        public Task HandleRemovedAsync(string id, App app, ConfiguredIntegration configured,
            CancellationToken ct = default)
        {
            var integration = appIntegrations.FirstOrDefault(x => x.Definition.Type == configured.Type);

            if (integration == null)
            {
                var error = string.Format(CultureInfo.InvariantCulture, Texts.IntegrationNotFound, configured.Type);

                throw new ValidationException(error);
            }

            return integration.OnRemovedAsync(app, id, configured, ct);
        }

        public bool IsConfigured<T>(App app, bool? test = null)
        {
            foreach (var (actualId, configured, integration) in GetIntegrations(app, test))
            {
                if (integration.CanCreate(typeof(T), actualId, configured))
                {
                    return true;
                }
            }

            return false;
        }

        public T? Resolve<T>(string id, App app, bool? test = null) where T : class
        {
            foreach (var (actualId, configured, integration) in GetIntegrations(app, test))
            {
                if (IsMatch(id, actualId) && integration.Create(typeof(T), actualId, configured, serviceProvider) is T created)
                {
                    return created;
                }
            }

            return default;
        }

        public IEnumerable<(string, T)> Resolve<T>(App app, bool? test = null) where T : class
        {
            foreach (var (actualId, configured, integration) in GetIntegrations(app, test))
            {
                if (integration.Create(typeof(T), actualId, configured, serviceProvider) is T created)
                {
                    yield return (actualId, created);
                }
            }

            yield break;
        }

        private IEnumerable<(string, ConfiguredIntegration, IIntegration)> GetIntegrations(App app, bool? test)
        {
            var configureds = app.Integrations;

            foreach (var (actualId, configured) in configureds)
            {
                if (!IsReady(configured) || !IsMatchingTest(configured, test))
                {
                    continue;
                }

                var integration = appIntegrations.FirstOrDefault(x => x.Definition.Type == configured.Type);

                if (integration != null)
                {
                    yield return (actualId, configured, integration);
                }
            }
        }

        private static bool IsReady(ConfiguredIntegration configured)
        {
            return configured.Enabled && configured.Status == IntegrationStatus.Verified;
        }

        private static bool IsMatchingTest(ConfiguredIntegration configured, bool? test)
        {
            return test == null || configured.Test == null || configured.Test.Value == test;
        }

        private static bool IsMatch(string id, string actualId)
        {
            return actualId == id;
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

                        var integration = appIntegrations.FirstOrDefault(x => x.Definition.Type == configured.Type);

                        if (integration == null)
                        {
                            updates[id] = IntegrationStatus.Verified;
                            continue;
                        }

                        try
                        {
                            await integration.CheckStatusAsync(app, id, configured, ct);
                        }
                        catch (Exception ex)
                        {
                            log.LogError(ex, w => w
                                .WriteProperty("action", "CheckIntegrations")
                                .WriteProperty("status", "Failed"));
                        }

                        if (status != configured.Status)
                        {
                            updates[id] = IntegrationStatus.Verified;
                        }
                    }

                    if (updates.Count > 0)
                    {
                        var command = new UpdateAppIntegrationStatus
                        {
                            Status = updates
                        };

                        await appStore.UpsertAsync(app.Id, command, ct);
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("action", "CheckIntegrations")
                    .WriteProperty("status", "Failed"));
            }
        }
    }
}

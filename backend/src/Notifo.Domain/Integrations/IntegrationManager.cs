// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Notifo.Domain.Apps;
using Notifo.Infrastructure.Timers;
using Notifo.Infrastructure.Validation;
using Squidex.Hosting;
using Squidex.Log;

namespace Notifo.Domain.Integrations
{
    public sealed class IntegrationManager : IIntegrationManager, IInitializable
    {
        private readonly IEnumerable<IIntegration> integrations;
        private readonly IAppStore appStore;
        private readonly ISemanticLog log;
        private CompletionTimer timer;

        public IEnumerable<IntegrationDefinition> Definitions
        {
            get => integrations.Select(x => x.Definition);
        }

        public IntegrationManager(IEnumerable<IIntegration> integrations, IAppStore appStore, ISemanticLog log)
        {
            this.integrations = integrations;

            this.appStore = appStore;

            this.log = log;
        }

        public Task InitializeAsync(CancellationToken ct)
        {
            timer = new CompletionTimer(5000, CheckAsync, 5000);

            return Task.CompletedTask;
        }

        public Task ReleaseAsync(CancellationToken ct)
        {
            return timer.StopAsync();
        }

        public Task HandleConfigured(ConfiguredIntegration configured, ConfiguredIntegration? previous)
        {
            var integration = integrations.FirstOrDefault(x => x.Definition.Type == configured.Type);

            if (integration == null)
            {
                throw new ValidationException("Integration type is not valid.");
            }

            return integration.OnConfiguredAsync(configured, previous);
        }

        public bool IsConfigured<T>(App app, bool test)
        {
            foreach (var (configured, integration) in GetIntegrations(app, test))
            {
                if (integration.CanCreate(typeof(T), configured))
                {
                    return true;
                }
            }

            return false;
        }

        public IEnumerable<T> Resolve<T>(App app, bool test) where T : class
        {
            foreach (var (configured, integration) in GetIntegrations(app, test))
            {
                if (integration.Create(typeof(T), configured) is T created)
                {
                    yield return created;
                }
            }
        }

        private IEnumerable<(ConfiguredIntegration Configured, IIntegration)> GetIntegrations(App app, bool test)
        {
            var configureds = app.Integrations;

            foreach (var (_, configured) in configureds)
            {
                if (!IsReady(configured) || !IsMatchingTest(configured, test))
                {
                    continue;
                }

                var integration = integrations.FirstOrDefault(x => x.Definition.Type == configured.Type);

                if (integration != null)
                {
                    yield return (configured, integration);
                }
            }
        }

        private static bool IsReady(ConfiguredIntegration configured)
        {
            return configured.Enabled && configured.Status == IntegrationStatus.Verified;
        }

        private static bool IsMatchingTest(ConfiguredIntegration configured, bool test)
        {
            return configured.Test == null || configured.Test.Value == test;
        }

        public async Task CheckAsync(CancellationToken ct)
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

                        var integration = integrations.FirstOrDefault(x => x.Definition.Type == configured.Type);

                        if (integration == null)
                        {
                            updates[id] = IntegrationStatus.Verified;
                            continue;
                        }

                        await integration.CheckStatusAsync(configured);

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

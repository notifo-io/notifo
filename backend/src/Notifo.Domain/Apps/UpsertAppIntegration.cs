// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Notifo.Domain.Integrations;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Collections;
using Notifo.Infrastructure.Validation;

namespace Notifo.Domain.Apps
{
    public sealed class UpsertAppIntegration : ICommand<App>
    {
        public string Id { get; set; }

        public string Type { get; set; }

        public bool Enabled { get; set; }

        public bool? Test { get; set; }

        public int Priority { get; set; }

        public ReadonlyDictionary<string, string> Properties { get; set; }

        private sealed class CreateValidator : AbstractValidator<UpsertAppIntegration>
        {
            public CreateValidator()
            {
                RuleFor(x => x.Id).NotNull();
                RuleFor(x => x.Type).NotNull();
                RuleFor(x => x.Properties).NotNull();
            }
        }

        private sealed class UpdateValidator : AbstractValidator<UpsertAppIntegration>
        {
            public UpdateValidator()
            {
                RuleFor(x => x.Id).NotNull();
                RuleFor(x => x.Properties).NotNull();
            }
        }

        public async ValueTask<App?> ExecuteAsync(App app, IServiceProvider serviceProvider,
            CancellationToken ct)
        {
            var integrationManager = serviceProvider.GetRequiredService<IIntegrationManager>();

            ConfiguredIntegration configured;

            if (app.Integrations.TryGetValue(Id, out var previousIntegration))
            {
                Validate<UpdateValidator>.It(this);

                configured = previousIntegration with { Properties = Properties };
            }
            else
            {
                Validate<CreateValidator>.It(this);

                configured = new ConfiguredIntegration(Type, Properties);
            }

            if (Test != configured.Test || Enabled != configured.Enabled || Priority != configured.Priority)
            {
                configured = configured with { Test = Test, Enabled = Enabled, Priority = Priority };
            }

            await integrationManager.ValidateAsync(configured, ct);
            await integrationManager.HandleConfiguredAsync(Id, app, configured, previousIntegration, ct);

            var newIntegrations = new Dictionary<string, ConfiguredIntegration>(app.Integrations)
            {
                [Id] = configured
            };

            var newApp = app with
            {
                Integrations = newIntegrations.ToImmutableDictionary()
            };

            return newApp;
        }
    }
}

// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Notifo.Domain.Integrations;
using Notifo.Infrastructure;
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

        public IntegrationProperties Properties { get; set; }

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

        public async Task<bool> ExecuteAsync(App app, IServiceProvider serviceProvider,
            CancellationToken ct)
        {
            ConfiguredIntegration configuration;

            if (app.Integrations.TryGetValue(Id, out var previousIntegration))
            {
                Validate<UpdateValidator>.It(this);

                configuration = previousIntegration with { Properties = Properties };
            }
            else
            {
                Validate<CreateValidator>.It(this);

                configuration = new ConfiguredIntegration(Type, Properties);
            }

            configuration.Test = Test;
            configuration.Enabled = Enabled;
            configuration.Priority = Priority;

            app.Integrations[Id] = configuration;

            var integrationManager = serviceProvider.GetRequiredService<IIntegrationManager>();

            await integrationManager.HandleConfigured(configuration, previousIntegration);

            return true;
        }
    }
}

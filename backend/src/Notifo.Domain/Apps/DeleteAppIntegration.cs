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
    public sealed class DeleteAppIntegration : ICommand<App>
    {
        public string Id { get; set; }

        private sealed class Validator : AbstractValidator<DeleteAppIntegration>
        {
            public Validator()
            {
                RuleFor(x => x.Id).NotNull();
            }
        }

        public async Task<bool> ExecuteAsync(App app, IServiceProvider serviceProvider,
            CancellationToken ct)
        {
            Validate<Validator>.It(this);

            var integrationManager = serviceProvider.GetRequiredService<IIntegrationManager>();

            if (app.Integrations.TryGetValue(Id, out var configured))
            {
                await integrationManager.HandleRemovedAsync(Id, app, configured, ct);
            }

            return app.Integrations.Remove(Id);
        }
    }
}

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
        private ConfiguredIntegration? removed;

        public string Id { get; set; }

        private sealed class Validator : AbstractValidator<DeleteAppIntegration>
        {
            public Validator()
            {
                RuleFor(x => x.Id).NotNull();
            }
        }

        public Task<bool> ExecuteAsync(App app, IServiceProvider serviceProvider,
            CancellationToken ct)
        {
            Validate<Validator>.It(this);

            return Task.FromResult(app.Integrations.TryRemove(Id, out removed));
        }

        public async Task ExecutedAsync(App app, IServiceProvider serviceProvider,
            CancellationToken ct)
        {
            if (removed == null)
            {
                return;
            }

            var integrationManager = serviceProvider.GetRequiredService<IIntegrationManager>();

            await integrationManager.HandleRemovedAsync(Id, app, removed, ct);
        }
    }
}

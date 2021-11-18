// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Notifo.Domain.Integrations;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Collections;
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

        public async ValueTask<App?> ExecuteAsync(App app, IServiceProvider serviceProvider,
            CancellationToken ct)
        {
            Validate<Validator>.It(this);

            if (!app.Integrations.TryGetValue(Id, out var removed))
            {
                return default;
            }

            var integrationManager = serviceProvider.GetRequiredService<IIntegrationManager>();

            await integrationManager.HandleRemovedAsync(Id, app, removed, ct);

            var newApp = app with
            {
                Integrations = app.Integrations.Where(x => x.Key != Id).ToReadonlyDictionary()
            };

            return newApp;
        }
    }
}

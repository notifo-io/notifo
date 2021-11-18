// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FluentValidation;
using Notifo.Domain.Integrations;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Collections;
using Notifo.Infrastructure.Validation;

namespace Notifo.Domain.Apps
{
    public sealed class UpdateAppIntegrationStatus : ICommand<App>
    {
        public Dictionary<string, IntegrationStatus> Status { get; set; }

        private sealed class Validator : AbstractValidator<UpdateAppIntegrationStatus>
        {
            public Validator()
            {
                RuleFor(x => x.Status).NotNull();
            }
        }

        public ValueTask<App?> ExecuteAsync(App app, IServiceProvider serviceProvider,
            CancellationToken ct)
        {
            Validate<Validator>.It(this);

            var newIntegrations = new Dictionary<string, ConfiguredIntegration>(app.Integrations);

            foreach (var (id, status) in Status)
            {
                if (newIntegrations.TryGetValue(id, out var current) && current.Status != status)
                {
                    newIntegrations[id] = current with { Status = status };
                }
            }

            var newApp = app with
            {
                Integrations = newIntegrations.ToReadonlyDictionary()
            };

            return new ValueTask<App?>(newApp);
        }
    }
}

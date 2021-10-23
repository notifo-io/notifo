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
using Notifo.Domain.Integrations;
using Notifo.Infrastructure;
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

        public Task<bool> ExecuteAsync(App app, IServiceProvider serviceProvider,
            CancellationToken ct)
        {
            Validate<Validator>.It(this);

            var isChanged = false;

            foreach (var (id, status) in Status)
            {
                if (app.Integrations.TryGetValue(id, out var current) && current.Status != status)
                {
                    app.Integrations[id] = current with { Status = status };

                    isChanged = true;
                }
            }

            return Task.FromResult(isChanged);
        }
    }
}

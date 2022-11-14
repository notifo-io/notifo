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

namespace Notifo.Domain.Apps;

public sealed class UpdateAppIntegrationStatus : AppCommand
{
    public Dictionary<string, IntegrationStatus> Status { get; set; }

    private sealed class Validator : AbstractValidator<UpdateAppIntegrationStatus>
    {
        public Validator()
        {
            RuleFor(x => x.Status).NotNull();
        }
    }

    public override ValueTask<App?> ExecuteAsync(App target, IServiceProvider serviceProvider,
        CancellationToken ct)
    {
        Validate<Validator>.It(this);

        var newIntegrations = target.Integrations.ToMutable();

        foreach (var (id, status) in Status)
        {
            if (newIntegrations.TryGetValue(id, out var current) && current.Status != status)
            {
                newIntegrations[id] = current with { Status = status };
            }
        }

        var newApp = target with
        {
            Integrations = newIntegrations.ToReadonlyDictionary()
        };

        return new ValueTask<App?>(newApp);
    }
}

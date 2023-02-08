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

namespace Notifo.Domain.Apps;

public sealed class DeleteAppIntegration : AppCommand
{
    public string IntegrationId { get; set; }

    private sealed class Validator : AbstractValidator<DeleteAppIntegration>
    {
        public Validator()
        {
            RuleFor(x => x.IntegrationId).NotNull();
        }
    }

    public override async ValueTask<App?> ExecuteAsync(App target, IServiceProvider serviceProvider,
        CancellationToken ct)
    {
        Validate<Validator>.It(this);

        if (!target.Integrations.TryGetValue(IntegrationId, out var removed))
        {
            return default;
        }

        var integrationManager = serviceProvider.GetRequiredService<IIntegrationManager>();

        await integrationManager.OnRemoveAsync(IntegrationId, target, removed, ct);

        var newApp = target with
        {
            Integrations = target.Integrations.Where(x => x.Key != IntegrationId).ToReadonlyDictionary()
        };

        return newApp;
    }
}

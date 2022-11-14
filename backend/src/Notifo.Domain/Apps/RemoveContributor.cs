// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FluentValidation;
using Notifo.Domain.Resources;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Collections;
using Notifo.Infrastructure.Validation;

namespace Notifo.Domain.Apps;

public sealed class RemoveContributor : AppCommand
{
    public string ContributorId { get; set; }

    private sealed class Validator : AbstractValidator<RemoveContributor>
    {
        public Validator()
        {
            RuleFor(x => x.ContributorId).NotNull().NotEmpty();
            RuleFor(x => x.PrincipalId).NotNull().NotEmpty();
        }
    }

    public override ValueTask<App?> ExecuteAsync(App target, IServiceProvider serviceProvider,
        CancellationToken ct)
    {
        Validate<Validator>.It(this);

        if (string.Equals(ContributorId, PrincipalId, StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException(Texts.App_CannotRemoveYourself);
        }

        if (!target.Contributors.ContainsKey(ContributorId))
        {
            return default;
        }

        var newApp = target with
        {
            Contributors = target.Contributors.Where(x => x.Key != ContributorId).ToReadonlyDictionary()
        };

        return new ValueTask<App?>(newApp);
    }
}

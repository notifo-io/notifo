// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FluentValidation;
using Notifo.Infrastructure.Validation;

namespace Notifo.Domain.Apps;

public sealed class UpsertAppAuthScheme : AppCommand
{
    public AppAuthScheme? Scheme { get; set; }

    private sealed class Validator : AbstractValidator<UpsertAppAuthScheme>
    {
        public Validator()
        {
            When(x => x.Scheme != null, () =>
            {
                RuleFor(x => x.Scheme).SetValidator(new SchemeValidator()!);
            });
        }
    }

    private sealed class SchemeValidator : AbstractValidator<AppAuthScheme>
    {
        public SchemeValidator()
        {
            RuleFor(x => x.Domain).NotNull().NotEmpty().Domain();
            RuleFor(x => x.DisplayName).NotNull().NotEmpty();
            RuleFor(x => x.ClientId).NotNull().NotEmpty();
            RuleFor(x => x.ClientSecret).NotNull().NotEmpty();
            RuleFor(x => x.Authority).NotNull().NotEmpty().Url();
            RuleFor(x => x.SignoutRedirectUrl).Url();
        }
    }

    public override ValueTask<App?> ExecuteAsync(App target, IServiceProvider serviceProvider,
        CancellationToken ct)
    {
        Validate<Validator>.It(this);

        if (!Equals(target.AuthScheme, Scheme))
        {
            target = target with { AuthScheme = Scheme };
        }

        return new ValueTask<App?>(target);
    }
}

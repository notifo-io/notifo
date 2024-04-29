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
    public string Domain { get; init; }

    public string DisplayName { get; init; }

    public string ClientId { get; init; }

    public string ClientSecret { get; init; }

    public string Authority { get; init; }

    public string? SignoutRedirectUrl { get; init; }

    private sealed class Validator : AbstractValidator<UpsertAppAuthScheme>
    {
        public Validator()
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

        var newScheme = new AppAuthScheme
        {
            Authority = Authority,
            ClientId = ClientId,
            ClientSecret = ClientSecret,
            DisplayName = DisplayName,
            Domain = Domain,
            SignoutRedirectUrl = SignoutRedirectUrl,
        };

        if (!Equals(target.AuthScheme, newScheme))
        {
            target = target with { AuthScheme = newScheme };
        }

        return new ValueTask<App?>(target);
    }
}

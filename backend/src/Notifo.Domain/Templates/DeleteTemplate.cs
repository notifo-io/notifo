// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;

namespace Notifo.Domain.Templates;

public sealed class DeleteTemplate : TemplateCommand
{
    public override bool IsUpsert => false;

    public override async ValueTask ExecuteAsync(IServiceProvider serviceProvider,
        CancellationToken ct)
    {
        await serviceProvider.GetRequiredService<ITemplateRepository>()
            .DeleteAsync(AppId, TemplateCode, ct);
    }
}

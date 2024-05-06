// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Apps;

public sealed class DeleteAppAuthScheme : AppCommand
{
    public override ValueTask<App?> ExecuteAsync(App target, IServiceProvider serviceProvider,
        CancellationToken ct)
    {
        target = target with { AuthScheme = null };

        return new ValueTask<App?>(target);
    }
}

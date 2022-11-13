// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;

namespace Notifo.Domain.Users;

public sealed class DeleteUser : UserCommand
{
    public override bool IsUpsert => false;

    public override async ValueTask ExecuteAsync(IServiceProvider serviceProvider,
        CancellationToken ct)
    {
        await serviceProvider.GetRequiredService<IUserRepository>()
            .DeleteAsync(AppId, UserId, ct);
    }
}

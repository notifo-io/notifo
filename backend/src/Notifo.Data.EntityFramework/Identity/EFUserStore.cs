// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Notifo.Infrastructure;

namespace Notifo.Identity;

public sealed class EFUserStore<TContext>(TContext context, IdentityErrorDescriber? describer = null)
    : UserStore<IdentityUser, IdentityRole, TContext>(context, describer) where TContext : DbContext
{
    public override async Task AddToRoleAsync(IdentityUser user, string normalizedRoleName,
        CancellationToken cancellationToken = default)
    {
        // Roles are not managed in Notifo, therefore we create them implicitly like the other stores.
        if (!await Context.Set<IdentityRole>().AnyAsync(x => x.NormalizedName == normalizedRoleName, cancellationToken))
        {
            var role = new IdentityRole { Name = normalizedRoleName, NormalizedName = normalizedRoleName };
            try
            {
                Context.Set<IdentityRole>().Add(role);

                await Context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (ex.IsUniqueViolation())
            {
                // The role has been created in the meantime.
                Context.Entry(role).State = EntityState.Detached;
            }
        }

        await base.AddToRoleAsync(user, normalizedRoleName, cancellationToken);
    }
}

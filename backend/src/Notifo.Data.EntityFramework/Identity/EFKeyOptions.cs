// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Notifo.Infrastructure;
using OpenIddict.Server;

namespace Notifo.Identity;

public sealed class EFKeyOptions<TContext>(IDbContextFactory<TContext> dbContextFactory) : IConfigureOptions<OpenIddictServerOptions> where TContext : DbContext
{
    private const string DefaultId = "Default";

    public void Configure(OpenIddictServerOptions options)
    {
        using var dbContext = dbContextFactory.CreateDbContext();

        var key = dbContext.Set<EFKeyEntity>().FirstOrDefault(x => x.Id == DefaultId);
        if (key == null)
        {
            var securityKey = new RsaSecurityKey(RSA.Create(2048))
            {
                KeyId = RandomHash.New()
            };

            var parameters =
                securityKey.Rsa != null ?
                securityKey.Rsa.ExportParameters(true) :
                securityKey.Parameters;

            key = EFKeyEntity.Create(DefaultId, securityKey.KeyId, parameters);
            try
            {
                dbContext.Set<EFKeyEntity>().Add(key);
                dbContext.SaveChanges();
            }
            catch (DbUpdateException ex) when (ex.IsUniqueViolation())
            {
                // Another instance has created the key in the meantime.
                key = dbContext.Set<EFKeyEntity>().AsNoTracking().First(x => x.Id == DefaultId);
            }
        }

        var rsaKey = new RsaSecurityKey(key.ToParameters())
        {
            KeyId = key.KeyId
        };

        options.SigningCredentials.Add(
            new SigningCredentials(rsaKey,
                SecurityAlgorithms.RsaSha256));

        options.EncryptionCredentials.Add(new EncryptingCredentials(rsaKey,
            SecurityAlgorithms.RsaOAEP,
            SecurityAlgorithms.Aes256CbcHmacSha512));
    }
}

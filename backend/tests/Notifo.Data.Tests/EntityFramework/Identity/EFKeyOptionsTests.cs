// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Notifo.EntityFramework.TestHelpers;
using Notifo.Identity;
using OpenIddict.Server;

namespace Notifo.EntityFramework.Identity;

public abstract class EFKeyOptionsTests<TContext>(ISqlFixture<TContext> fixture) where TContext : DbContext
{
    [Fact]
    public void Should_create_signing_key_once_and_reuse_it()
    {
        var options1 = new OpenIddictServerOptions();
        var options2 = new OpenIddictServerOptions();

        new EFKeyOptions<TContext>(fixture.DbContextFactory).Configure(options1);
        new EFKeyOptions<TContext>(fixture.DbContextFactory).Configure(options2);

        var key1 = Assert.Single(options1.SigningCredentials).Key.KeyId;
        var key2 = Assert.Single(options2.SigningCredentials).Key.KeyId;

        Assert.NotNull(key1);
        Assert.Equal(key1, key2);
        Assert.Single(options1.EncryptionCredentials);
    }
}

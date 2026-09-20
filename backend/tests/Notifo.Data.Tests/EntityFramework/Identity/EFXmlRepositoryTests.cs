// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.EntityFrameworkCore;
using Notifo.EntityFramework.TestHelpers;
using Notifo.Identity;
using Notifo.Shared;

namespace Notifo.EntityFramework.Identity;

public abstract class EFXmlRepositoryTests<TContext>(ISqlFixture<TContext> fixture) : XmlRepositoryTests where TContext : DbContext
{
    protected override Task<IXmlRepository> CreateSutAsync()
    {
        var sut = new EFXmlRepository<TContext>(fixture.DbContextFactory);

        return Task.FromResult<IXmlRepository>(sut);
    }
}

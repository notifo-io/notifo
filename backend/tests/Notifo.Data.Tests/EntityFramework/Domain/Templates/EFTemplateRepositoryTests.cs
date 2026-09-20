// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Notifo.Domain.Templates;
using Notifo.EntityFramework.TestHelpers;
using Notifo.Shared;

namespace Notifo.EntityFramework.Domain.Templates;

public abstract class EFTemplateRepositoryTests<TContext>(ISqlFixture<TContext> fixture) : TemplateRepositoryTests where TContext : DbContext
{
    protected override Task<ITemplateRepository> CreateSutAsync()
    {
        var sut = new EFTemplateRepository<TContext>(fixture.DbContextFactory);

        return Task.FromResult<ITemplateRepository>(sut);
    }
}

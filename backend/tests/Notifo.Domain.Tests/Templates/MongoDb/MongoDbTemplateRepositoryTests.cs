// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Shared;
using Notifo.Infrastructure.Fixtures;

namespace Notifo.Domain.Templates.MongoDb;

[Trait("Category", "TestContainer")]
[Collection(MongoFixtureCollection.Name)]
public class MongoDbTemplateRepositoryTests(MongoFixture fixture) : TemplateRepositoryTests
{
    protected override async Task<ITemplateRepository> CreateSutAsync()
    {
        var sut = new MongoDbTemplateRepository(fixture.Database);

        await sut.InitializeAsync(default);
        return sut;
    }
}

// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Shared;
using Notifo.Infrastructure.Fixtures;

namespace Notifo.Domain.Apps.MongoDb;

[Trait("Category", "TestContainer")]
[Collection(MongoFixtureCollection.Name)]
public class MongoDbAppRepositoryTests(MongoFixture fixture) : AppRepositoryTests
{
    protected override async Task<IAppRepository> CreateSutAsync()
    {
        var sut = new MongoDbAppRepository(fixture.Database);

        await sut.InitializeAsync(default);
        return sut;
    }
}

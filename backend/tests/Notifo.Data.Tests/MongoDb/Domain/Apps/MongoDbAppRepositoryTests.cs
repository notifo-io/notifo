// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Apps;
using Notifo.MongoDb.TestHelpers;
using Notifo.Shared;

namespace Notifo.MongoDb.Domain.Apps;

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

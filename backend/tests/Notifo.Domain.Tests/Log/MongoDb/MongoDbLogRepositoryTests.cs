// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Shared;
using Notifo.Infrastructure.Fixtures;

namespace Notifo.Domain.Log.MongoDb;

[Trait("Category", "TestContainer")]
[Collection(MongoFixtureCollection.Name)]
public class MongoDbLogRepositoryTests(MongoFixture fixture) : LogRepositoryTests
{
    protected override async Task<ILogRepository> CreateSutAsync()
    {
        var sut = new MongoDbLogRepository(fixture.Database);

        await sut.InitializeAsync(default);
        return sut;
    }
}

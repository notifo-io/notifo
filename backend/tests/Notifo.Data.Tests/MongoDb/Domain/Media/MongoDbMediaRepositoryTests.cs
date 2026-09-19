// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Shared;
using Notifo.Infrastructure.Fixtures;

namespace Notifo.Domain.Media.MongoDb;

[Trait("Category", "TestContainer")]
[Collection(MongoFixtureCollection.Name)]
public class MongoDbMediaRepositoryTests(MongoFixture fixture) : MediaRepositoryTests
{
    protected override async Task<IMediaRepository> CreateSutAsync()
    {
        var sut = new MongoDbMediaRepository(fixture.Database);

        await sut.InitializeAsync(default);
        return sut;
    }
}

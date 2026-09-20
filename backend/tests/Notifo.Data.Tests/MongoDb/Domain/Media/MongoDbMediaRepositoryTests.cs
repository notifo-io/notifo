// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Media;
using Notifo.MongoDb.TestHelpers;
using Notifo.Shared;

namespace Notifo.MongoDb.Domain.Media;

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

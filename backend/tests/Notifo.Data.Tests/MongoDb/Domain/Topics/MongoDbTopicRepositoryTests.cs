// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Topics;
using Notifo.MongoDb.TestHelpers;
using Notifo.Shared;

namespace Notifo.MongoDb.Domain.Topics;

[Trait("Category", "TestContainer")]
[Collection(MongoFixtureCollection.Name)]
public class MongoDbTopicRepositoryTests(MongoFixture fixture) : TopicRepositoryTests
{
    protected override async Task<ITopicRepository> CreateSutAsync()
    {
        var sut = new MongoDbTopicRepository(fixture.Database);

        await sut.InitializeAsync(default);
        return sut;
    }
}

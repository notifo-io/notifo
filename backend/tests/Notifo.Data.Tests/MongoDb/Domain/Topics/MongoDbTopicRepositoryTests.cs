// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Shared;
using Notifo.Infrastructure.Fixtures;

namespace Notifo.Domain.Topics.MongoDb;

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
